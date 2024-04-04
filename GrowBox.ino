#include <List.hpp>
#include <Wire.h>
#include <Arduino_JSON.h>
#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <ESP8266WebServer.h>
#include <ESP8266mDNS.h>
#include <EEPROM.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>
#include <WiFiUdp.h>
#include <ArduinoOTA.h>
#include <RTClib.h>
#include "FS.h"
#include <LittleFS.h>

#include "index.h"
#include "favicon.h"

#include "arduino_secrets.h" 
// ######################################################
// # Please enter your sensitive data in the            #
// # arduino_secrets.h tab, if there is none create one #
// # using CTRL + SHIFT + N (Arduino IDE)               #
// # or    CTRL + N         (VSCode)                    #
// ######################################################

const char* hostname = "GrowBoxTest";
const char* ssid = SECRET_SSID;    // your network SSID (name)
const char* pass = SECRET_PASS;    // your network password

// Timezone rule / NTP Servers
const char* time_zone = "CET-1CEST,M3.5.0,M10.5.0/3"; // (Berlin)
#define NTP_SERVERS "0.de.pool.ntp.org", "1.de.pool.ntp.org", "2.de.pool.ntp.org"
#define NTP_MIN_VALID_EPOCH 1533081600

ESP8266WebServer server(80);
Adafruit_BME280 bme;

// Pins
  const int pBuildinLed = LED_BUILTIN;

  const int pFan = D0;
  const int pLight = D5;
  // TODO: BME280!, Supersonic? (plant height), Soil moisture?, EC / PH?, 
  // pumps/fert/water?, 

// BMEData
struct BMEData {
  time_t timestamp;
  float temperature;
  float pressure;
  float humidity;
};
const int BME_DATA_SIZE = sizeof(BMEData);
BMEData bmeData;
// time between retained measurements.
const int bmeDataRetentionDelay = 3600; // 1 hr
// max data retention.
const unsigned char bmeDataRetention = 24; // 24 samples so we get 1 day retained bme data
// retained bme data.
List<BMEData> bmeDataRetained;

// Context
struct Context
{
  // Fan Speed, 255 = OFF, 0 = MAX.
  unsigned char fanSpeed;

  unsigned char light;
  int32_t sunrise;
  int32_t sunDuration;
  int32_t sunriseSetDuration;
  unsigned char sunTargetLight;
  bool sunScheduleEnabled;
};
const int CONTEXT_SIZE = sizeof(Context);
const char * CONTEXT_MARKER = "GROW";
const int CONTEXT_MARKER_SIZE = 4;
Context context;
bool bmeAvailable = false;
bool fsMounted = false;

// Errors
const char * parsingFailed = "Parsing JSON input failed!";
const char * invalidRequest = "Invalid Request.";


// CONTEXT ##############################################
void contextInit() {
  EEPROM.begin(512);
  if (memcmp(CONTEXT_MARKER, EEPROM.getConstDataPtr(), CONTEXT_MARKER_SIZE) != 0) {
    Serial.println("No viable context found in EEPROM, initializing default.");
    // INITIALIZE CONTEXT DEFAULT VALUES HERE.
    
    // Fan Speed, 255 = OFF, 0 = MAX.
    context.fanSpeed = 255;
    // light, 255 = OFF, 0 = MAX.
    context.light = 255;

    context.sunrise = 6 * 60 * 60; // 21600
    context.sunDuration = 18 * 60 * 60; // 64800
    context.sunriseSetDuration = 8 * 60; // 480
    context.sunTargetLight = 255;
    context.sunScheduleEnabled = true;
    return;
  }
  memcpy(&context, EEPROM.getConstDataPtr() + CONTEXT_MARKER_SIZE, CONTEXT_SIZE);
  
  Serial.println("Context loaded from EEPROM");
}

void contextSaveChanges() {
  memcpy(EEPROM.getDataPtr(), CONTEXT_MARKER, CONTEXT_MARKER_SIZE);
  memcpy(EEPROM.getDataPtr() + CONTEXT_MARKER_SIZE, &context, CONTEXT_SIZE);
  EEPROM.commit();
}

void bmeRetentionInit() {
  Serial.print("bmeRetentionInit ");
  File file = LittleFS.open("bmeretention.bin", "r");
  if (!file) {
      Serial.println("- failed to open file for reading");
      return;
  }
  int size = file.read();
  for (int i = 0; i < size; i++)
  {
    BMEData data;
    file.read((uint8_t*)&data, BME_DATA_SIZE);
    bmeDataRetained.add(data);
  }
  file.close();    
  Serial.println("- file written");
}

void bmeRetentionDelete() {
  LittleFS.remove("bmeretention.bin");
  bmeDataRetained.clear();
}

void bmeRetentionSaveChanges() {
  if (bmeDataRetained.getSize() <= 0) {
    return;
  }
  Serial.print("bmeRetentionSaveChanges ");
  File file = LittleFS.open("bmeretention.bin", "w");
  if (!file) {
      Serial.println("- failed to open file for writing");
      return;
  }

  int size = bmeDataRetained.getSize();

  file.write(size);
  for (int i = 0; i < size; i++)
  {
    BMEData data = bmeDataRetained[i];
    file.write((uint8_t*)&data, BME_DATA_SIZE);
  }
  file.close();    
  Serial.println("- file written");
}

// SERVER I/O ###########################################

String bmeToJson(BMEData data) {
    String result = "{";
    result += "\"temperature\":" + String(data.temperature) + ",";
    result += "\"humidity\":" + String(data.humidity) + ",";
    result += "\"pressure\":" + String(data.pressure) + ",";
    result += "\"timestamp\":" + String(data.timestamp);
    result += "}";
    return result;
}

void serverSendContext() {
    String result = 
     "{";
        result += "\"me\": \"" + String(hostname) + "\",";
        result += "\"fsMounted\": \"" + String(fsMounted) + "\",";
        result += "\"light\": " + String(context.light) + ",";
        result += "\"fanSpeed\":" + String(context.fanSpeed);
        result += ", \"lightSchedule\": {";
          result += " \"sunScheduleEnabled\" :" + String(context.sunScheduleEnabled);
          result += ",\"sunrise\" :" + String(context.sunrise);
          result += ",\"sunDuration\" :" + String(context.sunDuration);
          result += ",\"sunriseSetDuration\" :" + String(context.sunriseSetDuration);
          result += ",\"sunTargetLight\" :" + String(context.sunTargetLight);
        result += "}";
      if (bmeAvailable) {
        result += ", \"bme\": " + bmeToJson(bmeData);
        result += ", \"bmeRetained\": [";
        for(int i = 0; i < bmeDataRetained.getSize(); i++) {
           result += (i != 0 ? ", " : "") + bmeToJson(bmeDataRetained[i]);
        }
        result += "]";
      }
    result += "}";

    server.send(200, "application/json", result.c_str());
}

bool serverParseJson(JSONVar* jsonInput) {
  *jsonInput = JSON.parse(server.arg("plain")); 
 
  // JSON.typeof(jsonVar) can be used to get the type of the variable 
  if (JSON.typeof(*jsonInput) == "undefined") { 
    Serial.println(parsingFailed);
    server.send(400, "text/plain", parsingFailed);
    return false; 
  }
  return true;
}

void serverSendInvalidRequest() {
  server.send(400, "text/plain", invalidRequest); 
}

// REQUEST HANDLERS #####################################

void handleRoot() {
  server.send(200, "text/html", HTML_INDEX);
}

void handleGet() {
  serverSendContext();
}

void handleFanSet() {
  Serial.println("Handling Fan Set Request"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;
   
  if (jsonInput.hasOwnProperty("fanSpeed")) { 
    context.fanSpeed = (unsigned char)jsonInput["fanSpeed"];
    Serial.print("fanSpeed set to ");
    Serial.println(context.fanSpeed);
    analogWrite(pFan, context.fanSpeed);
    contextSaveChanges();
    serverSendContext();
    return; 
  }
  serverSendInvalidRequest();
}

void handleLightSet() {
  Serial.println("Handling Light Set Request"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;
   
  if (jsonInput.hasOwnProperty("light")) { 
    context.light = (unsigned char)jsonInput["light"];
    Serial.print("light set to ");
    Serial.println(context.light);
    analogWrite(pLight, context.light);
    contextSaveChanges();
    serverSendContext();
    return; 
  }
  serverSendInvalidRequest();
}

void handleLightScheduleSet() {
  Serial.println("Handling Light Schedule Set Request"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;

  bool scheduleChanged = false;   
  if (jsonInput.hasOwnProperty("sunrise")) { 
    context.sunrise = (int32_t)jsonInput["sunrise"];
    Serial.print("sunrise set to ");
    Serial.println(context.sunrise);
    scheduleChanged = true;
  }
   
  if (jsonInput.hasOwnProperty("sunDuration")) { 
    context.sunDuration = (int32_t)jsonInput["sunDuration"];
    Serial.print("sunDuration set to ");
    Serial.println(context.sunDuration);
    scheduleChanged = true;
  }
   
  if (jsonInput.hasOwnProperty("sunriseSetDuration")) { 
    context.sunriseSetDuration = (int32_t)jsonInput["sunriseSetDuration"];
    Serial.print("sunriseSetDuration set to ");
    Serial.println(context.sunriseSetDuration);
    scheduleChanged = true;
  }
   
  if (jsonInput.hasOwnProperty("sunTargetLight")) { 
    context.sunTargetLight = (unsigned char)jsonInput["sunTargetLight"];
    Serial.print("sunTargetLight set to ");
    Serial.println(context.sunTargetLight);
    scheduleChanged = true;
  }
  if (jsonInput.hasOwnProperty("sunScheduleEnabled")) { 
    context.sunScheduleEnabled = (bool)jsonInput["sunScheduleEnabled"];
    Serial.print("sunScheduleEnabled set to ");
    Serial.println(context.sunScheduleEnabled);
    scheduleChanged = true;
  }

  if (scheduleChanged) {
    contextSaveChanges();
    serverSendContext();
    return;
  }
  serverSendInvalidRequest();
}

void handleNotFound() {
  String message = "File Not Found\n\n";
  message += "URI: ";
  message += server.uri();
  message += "\nMethod: ";
  message += (server.method() == HTTP_GET) ? "GET" : "POST";
  message += "\nArguments: ";
  message += server.args();
  message += "\n";
  for (uint8_t i = 0; i < server.args(); i++) { message += " " + server.argName(i) + ": " + server.arg(i) + "\n"; }
  server.send(404, "text/plain", message);
}

void handleClear() {
  bmeRetentionDelete();
  serverSendContext();
}

void handleFavIcon() {
  server.send(200, "image/x-icon", FAV_ICON, FAV_ICON_SIZE);
}

void configureRoutes() {
  server.on("/", handleRoot);
  server.on("/favicon.ico", handleFavIcon);
  server.on("/get", handleGet);
  server.on("/clear", handleClear);
  server.on("/fan/set", HTTP_POST, handleFanSet);
  server.on("/light/set", HTTP_POST, handleLightSet);
  server.on("/light/schedule/set", HTTP_POST, handleLightScheduleSet);
  
  server.onNotFound(handleNotFound);
}

// SETUP / LOOP #########################################

void initNtp() {
  time_t now;
  configTzTime(time_zone, NTP_SERVERS);
  Serial.print("Wait for valid ntp response.");
  while((now = time(nullptr)) < NTP_MIN_VALID_EPOCH) {
    blink(500);
    Serial.print(".");
  }
  Serial.println();
}

// blink with delay
void blink(unsigned int ms)
{
  unsigned int half = ms / 2;

  digitalWrite(pBuildinLed, LOW);
  delay(half);
  digitalWrite(pBuildinLed, HIGH);
  delay(half);
}

void readBMEData() {
  if (bmeAvailable) {
    bmeData.timestamp = time(nullptr);
    bmeData.humidity = bme.readHumidity();
    bmeData.pressure = bme.readPressure();
    bmeData.temperature = bme.readTemperature();

    if (bmeDataRetained.getSize() == 0 
      || bmeData.timestamp - bmeDataRetained[0].timestamp > bmeDataRetentionDelay) {
        // retain every hour or immidiatly for the first.
        bmeDataRetained.addFirst(bmeData);
        // truncate if enough
        if (bmeDataRetained.getSize() > bmeDataRetention) {
          bmeDataRetained.removeLast();
        }
        bmeRetentionSaveChanges();
    }
  }
}

void sunSchedule(DateTime now) {
  DateTime lastRise = DateTime(now.year(), now.month(), now.day()-1, 0, 0, 0) + context.sunrise;
  DateTime lastSet = lastRise + context.sunDuration;
  DateTime rise = DateTime(now.year(), now.month(), now.day(), 0, 0, 0) + context.sunrise;
  DateTime set = rise + context.sunDuration;
  char buf[20];

  if(lastSet > now) {
    rise = lastRise;
    set = lastSet;
  }
  
  double target = 0;
  bool riseOrSet = false;
  int32_t riseDiff = (now - rise).totalseconds();
  if (riseDiff > 0 && riseDiff <= context.sunriseSetDuration) {
    target = (double)riseDiff / (double)context.sunriseSetDuration;
    riseOrSet = true;
  } 
  int32_t setDiff = (now - (set - context.sunriseSetDuration)).totalseconds();
  if (setDiff > 0 && setDiff <= context.sunriseSetDuration) {
    target = 1.0 - ((double)setDiff / (double)context.sunriseSetDuration);
    riseOrSet = true;
  }

  if (!riseOrSet) {
    if (now >= rise && now < set) {
      target = 1;
    } else {
      target = 0;
    }
  }

  double dtl = target * (double)context.sunTargetLight;
  char targetLight = context.sunTargetLight - dtl;
  if (context.light != targetLight) {
    context.light = targetLight;
    analogWrite(pLight, context.light);
    contextSaveChanges();
  }
}

void setupOTA() {
  Serial.println("Initializing OTA.");
  ArduinoOTA.onStart([]() {
    String type;
    if (ArduinoOTA.getCommand() == U_FLASH) {
      type = "sketch";
    } else { // U_FS
      type = "filesystem";
    }
    Serial.println("Start updating " + type);
  });
  ArduinoOTA.onEnd([]() {
    Serial.println("\nEnd");
  });
  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
    Serial.printf("Progress: %u%%\n", (progress / (total / 100)));
  });
  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
    if (error == OTA_AUTH_ERROR) {
      Serial.println("Auth Failed");
    } else if (error == OTA_BEGIN_ERROR) {
      Serial.println("Begin Failed");
    } else if (error == OTA_CONNECT_ERROR) {
      Serial.println("Connect Failed");
    } else if (error == OTA_RECEIVE_ERROR) {
      Serial.println("Receive Failed");
    } else if (error == OTA_END_ERROR) {
      Serial.println("End Failed");
    }
  });
  ArduinoOTA.begin();
  Serial.println("OTA Available.");
}

void setup() {
  // static pin init.
  pinMode(pBuildinLed, OUTPUT);
  digitalWrite(pBuildinLed, HIGH);
  // serial
  Serial.begin(115200);
  Serial.println("");
  Serial.flush();
  if(LittleFS.begin()) {
    Serial.println("LittleFS Mount Failed");
    fsMounted = true;
  } else {
    fsMounted = false;
  }

  // init retention
  bmeRetentionInit();

  // init context
  contextInit();

  // then init context driven pins.
  pinMode(pFan, OUTPUT);
  analogWrite(pFan, context.fanSpeed);
  pinMode(pLight, OUTPUT);
  analogWrite(pLight, context.light);

  // init bme
  if (!bme.begin(0x76)) {
    Serial.println("BME280 missing!");
    bmeAvailable = false;
  } else {
    Serial.println("BME280 Initialized.");
    bmeAvailable = true;
  }
  
  // connect wifi
  Serial.println("Connecting");
  WiFi.hostname(hostname);
  WiFi.begin(ssid, pass);
  while (WiFi.status() != WL_CONNECTED) {
    blink(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.print("Connected to ");
  Serial.println(ssid);
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
  Serial.print("Hostname: ");
  Serial.printf("http://%s/\n", hostname);
  
  // AutoReconnect
  WiFi.setAutoReconnect(true);
  WiFi.persistent(true);
  
  // start mdns responder hostname is better than ip ;)
  if (MDNS.begin(hostname)) 
  { 
    Serial.println("MDNS responder started"); 
  }

  // initialize ntp
  initNtp();

  // read initial bme after ntp init.
  readBMEData();

  // configure routes
  configureRoutes();

  // launch server.
  server.begin();

  setupOTA();

  Serial.println("HTTP server started");
  blink(2000);
}

time_t lastEpoch = 0;
void loop() {
  time_t epoch = time(nullptr);
  DateTime now = DateTime(epoch);
  bool epochChanged = epoch != lastEpoch;

  // update mdns
  MDNS.update();

  // handle ota
  ArduinoOTA.handle();
  
  // handle client requests
  server.handleClient();

  if (epochChanged && epoch % 2 == 0) {
    if (bmeAvailable) {
      readBMEData();
    }

    if (context.sunScheduleEnabled) {
      sunSchedule(now);
    }
  }

  if (epochChanged && epoch % 60 == 0) {
    initNtp();
  }

  lastEpoch = epoch;
  delay(10);
}
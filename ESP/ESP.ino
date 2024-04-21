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
#include "time.h"
#include "arduino_secrets.h" 
// ######################################################
// # Please enter your sensitive data in the            #
// # arduino_secrets.h tab, if there is none create one #
// # using CTRL + SHIFT + N (Arduino IDE)               #
// # or    CTRL + N         (VSCode)                    #
// ######################################################

const char* hostname = "GrowBox01";
const char* ssid = SECRET_SSID;    // your network SSID (name)
const char* pass = SECRET_PASS;    // your network password
const char* webcam = "http://192.168.178.200:8081/";

// Timezone rule / NTP Servers
const char* time_zone = "CET-1CEST,M3.5.0,M10.5.0/3"; // (Berlin)
const long  gmtOffset_sec = 3600;
const int   daylightOffset_sec = 3600;
#define NTP_SERVERS "0.de.pool.ntp.org", "1.de.pool.ntp.org", "2.de.pool.ntp.org"
#define NTP_MIN_VALID_EPOCH 1533081600
#define mytime time(nullptr) + 7200

ESP8266WebServer server(80);
Adafruit_BME280 bme;

// Pins
  const int pBuildinLed = LED_BUILTIN;

  const int pFan = D0;
  const int pLight = D5;
  // TODO: Supersonic? (plant height), Soil moisture?, EC / PH?, 
  // pumps/fert/water?, 

// Context
struct Context
{
  // Fan Speed, 255 = OFF, 0 = MAX.
  unsigned char fanSpeed;
  unsigned char light;
  int32_t sunrise;
  int32_t sunDuration;
  unsigned char sunTargetLight;
  bool sunScheduleEnabled;
  time_t timestamp;
  float temperature;
  float pressure;
  float humidity;
  float dewPoint;
  float heatIndex;
};
const int CONTEXT_SIZE = sizeof(Context);
const char * CONTEXT_MARKER = "GROO";
const int CONTEXT_MARKER_SIZE = 4;
Context context;
bool bmeAvailable = false;
bool fsMounted = false;
DateTime rise;
DateTime set;
DateTime now;

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
    context.fanSpeed = 186;
    // light, 255 = OFF, 0 = MAX.
    context.light = 103;

    context.sunrise = 6 * 60 * 60; // 21600
    context.sunDuration = 18 * 60 * 60; // 64800
    context.sunTargetLight = 103;
    context.sunScheduleEnabled = true;
    context.dewPoint = 0;
    context.heatIndex = 0;
    context.humidity = 0;
    context.temperature = 0;
    context.timestamp = 0;
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

// SERVER I/O ###########################################

void serverSendContext() {
  char buff[20];
    String result = "{";
        result += "\"me\": \"" + String(hostname) + "\",";
        result += "\"light\": " + String(context.light) + ",";
        result += "\"temperature\":" + String(context.temperature) + ",";
        result += "\"humidity\":" + String(context.humidity) + ",";
        result += "\"pressure\":" + String(context.pressure) + ",";
        result += "\"timestamp\":" + String(context.timestamp) + ",";
        result += "\"dewPoint\":" + String(context.dewPoint) + ",";
        result += "\"heatIndex\":" + String(context.heatIndex) + ",";
        result += "\"timestamp\":" + String(context.timestamp) + ",";
        result += "\"fanSpeed\":" + String(context.fanSpeed);

        result += ", \"lightSchedule\": {";
          result += " \"sunScheduleEnabled\" :" + String(context.sunScheduleEnabled ? "true" : "false");
          result += ",\"sunrise\" :" + String(context.sunrise);
          result += ",\"sunDuration\" :" + String(context.sunDuration);
          result += ",\"sunTargetLight\" :" + String(context.sunTargetLight);
        result += "}";

    result += "}";
    
    server.sendHeader(String(F("Access-Control-Allow-Private-Network")), String("true"));
    server.send(200, "application/json", result.c_str());
}

bool serverParseJson(JSONVar* jsonInput) {
  *jsonInput = JSON.parse(server.arg("plain")); 
 
  // JSON.typeof(jsonVar) can be used to get the type of the variable 
  if (JSON.typeof(*jsonInput) == "undefined") { 
    Serial.println(parsingFailed);
    server.sendHeader(String(F("Access-Control-Allow-Private-Network")), String("true"));
    server.send(400, "text/plain", parsingFailed);
    return false; 
  }
  return true;
}

void serverSendInvalidRequest() {
  server.sendHeader(String(F("Access-Control-Allow-Private-Network")), String("true"));
  server.send(400, "text/plain", invalidRequest); 
}

// REQUEST HANDLERS #####################################
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
  if (server.method() == HTTP_OPTIONS)
  {
    server.sendHeader("Access-Control-Max-Age", "10000");
    server.sendHeader("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE");
    server.sendHeader("Access-Control-Allow-Headers", "*");
    server.sendHeader(String(F("Access-Control-Allow-Private-Network")), String("true"));
    server.send(200);
    return;
  }
  String message = "File Not Found\n\n";
  message += "URI: ";
  message += server.uri();
  message += "\nMethod: ";
  message += (server.method() == HTTP_GET) ? "GET" : "POST";
  message += "\nArguments: ";
  message += server.args();
  message += "\n";
  for (uint8_t i = 0; i < server.args(); i++) { message += " " + server.argName(i) + ": " + server.arg(i) + "\n"; }
  server.sendHeader(String(F("Access-Control-Allow-Private-Network")), String("true"));
  server.send(404, "text/plain", message);
}

void configureRoutes() {
  server.enableCORS(true);
  
  server.on("/get", handleGet);
  server.on("/fan/set", HTTP_POST, handleFanSet);
  server.on("/light/set", HTTP_POST, handleLightSet);
  server.on("/light/schedule/set", HTTP_POST, handleLightScheduleSet);

  server.onNotFound(handleNotFound);
}

// SETUP / LOOP #########################################

void initNtp() {
  time_t n;
  configTime(0, 0, NTP_SERVERS);
  setenv("TZ", time_zone, 1);     
  tzset();
  Serial.print("Wait for valid ntp response.");
  while((n = mytime) < NTP_MIN_VALID_EPOCH) {
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

#define hi_coeff1 -42.379
#define hi_coeff2   2.04901523
#define hi_coeff3  10.14333127
#define hi_coeff4  -0.22475541
#define hi_coeff5  -0.00683783
#define hi_coeff6  -0.05481717
#define hi_coeff7   0.00122874
#define hi_coeff8   0.00085282
#define hi_coeff9  -0.00000199
float HeatIndex(float temperature, float humidity)
{
  float heatIndex(NAN);
  if (isnan(temperature) || isnan(humidity)) 
  {
    return heatIndex;
  }

  temperature = (temperature * (9.0 / 5.0) + 32.0); /*conversion to [°F]*/
  // Using both Rothfusz and Steadman's equations
  // http://www.wpc.ncep.noaa.gov/html/heatindex_equation.shtml
  if (temperature <= 40) 
  {
    heatIndex = temperature;	//first red block
  }
  else
  {
    heatIndex = 0.5 * (temperature + 61.0 + ((temperature - 68.0) * 1.2) + (humidity * 0.094));	//calculate A -- from the official site, not the flow graph

    if (heatIndex >= 79) 
    {
      /*
      * calculate B  
      * the following calculation is optimized. Simply spoken, reduzed cpu-operations to minimize used ram and runtime. 
      * Check the correctness with the following link:
      * http://www.wolframalpha.com/input/?source=nav&i=b%3D+x1+%2B+x2*T+%2B+x3*H+%2B+x4*T*H+%2B+x5*T*T+%2B+x6*H*H+%2B+x7*T*T*H+%2B+x8*T*H*H+%2B+x9*T*T*H*H
      */
      heatIndex = hi_coeff1
      + (hi_coeff2 + hi_coeff4 * humidity + temperature * (hi_coeff5 + hi_coeff7 * humidity)) * temperature
      + (hi_coeff3 + humidity * (hi_coeff6 + temperature * (hi_coeff8 + hi_coeff9 * temperature))) * humidity;
      //third red block
      if ((humidity < 13) && (temperature >= 80.0) && (temperature <= 112.0))
      {
        heatIndex -= ((13.0 - humidity) * 0.25) * sqrt((17.0 - abs(temperature - 95.0)) * 0.05882);
      } //fourth red block
      else if ((humidity > 85.0) && (temperature >= 80.0) && (temperature <= 87.0))
      {
        heatIndex += (0.02 * (humidity - 85.0) * (87.0 - temperature));
      }
    }
  }
  return (heatIndex - 32.0) * (5.0 / 9.0); /*conversion back to [°C]*/
}

float DewPoint(float temp, float hum)
{
  // Equations courtesy of Brian McNoldy from http://andrew.rsmas.miami.edu;
  float dewPoint = NAN;
  if(!isnan(temp) && !isnan(hum))
  {
       dewPoint = 243.04 * (log(hum/100.0) + ((17.625 * temp)/(243.04 + temp)))
       /(17.625 - log(hum/100.0) - ((17.625 * temp)/(243.04 + temp)));
  }

  return dewPoint;
}

void updateContext() {
  context.timestamp = mytime;  
  if (bmeAvailable) {
    context.humidity = bme.readHumidity();
    context.pressure = bme.readPressure();
    context.temperature = bme.readTemperature();
    context.dewPoint = DewPoint(context.temperature, context.humidity);
    context.heatIndex = HeatIndex(context.temperature, context.humidity);
  } else {
    context.humidity = 0;
    context.pressure = 0;
    context.temperature = 0;
    context.dewPoint = 0;
    context.heatIndex = 0;
  }
}

void sunSchedule() {
  DateTime lastRise = DateTime(now.year(), now.month(), now.day()-1, 0, 0, 0) + context.sunrise;
  DateTime lastSet = lastRise + context.sunDuration;
  DateTime nowrise = DateTime(now.year(), now.month(), now.day(), 0, 0, 0) + context.sunrise;
  DateTime nowset = nowrise + context.sunDuration;
  char buff[20];
  Serial.println("SUN SCHEDULE ####################");
  Serial.printf("NOW: %s", now.tostr(buff)); Serial.println();
  Serial.printf("Last Rise: %s", lastRise.tostr(buff)); Serial.println();
  Serial.printf("Last Set: %s", lastSet.tostr(buff)); Serial.println();
  Serial.printf("Rise: %s", rise.tostr(buff)); Serial.println();
  Serial.printf("Set: %s", set.tostr(buff)); Serial.println();
  if(lastSet > now) {
    rise = lastRise;
    set = lastSet;
  } else {
    rise = nowrise;
    set = nowset;
  }
  Serial.printf("Picked Rise: %s", rise.tostr(buff)); Serial.println();
  Serial.printf("Picked Set: %s", set.tostr(buff)); Serial.println();
  char targetLight = 255;
  if (now > rise && now <= set) { 
    targetLight = context.sunTargetLight;
  }
  Serial.printf("targetLight = %d", (int)targetLight); Serial.println();
  if (context.light != targetLight) {
    context.light = targetLight;
    analogWrite(pLight, context.light);
    contextSaveChanges();
  }
  Serial.println("#################################");
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

  // update context 
  updateContext();

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
  time_t epoch = mytime;
  now = DateTime(epoch);
  bool epochChanged = epoch != lastEpoch;

  // update mdns
  MDNS.update();

  // handle ota
  ArduinoOTA.handle();
  
  // handle client requests
  server.handleClient();

  if (epochChanged && epoch % 2 == 0) {
    updateContext();
    if (context.sunScheduleEnabled) {
      sunSchedule();
    }
  }

  if (epochChanged && epoch % 60 == 0) {
    initNtp();
  }

  lastEpoch = epoch;
  delay(10);
}
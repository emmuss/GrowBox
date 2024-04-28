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

const char* hostname = "WaterPump01";
const char* ssid = SECRET_SSID;    // your network SSID (name)
const char* pass = SECRET_PASS;    // your network password

// Timezone rule / NTP Servers
//const char* time_zone = "CET-1CEST,M3.5.0,M10.5.0/3"; // (Berlin)
#define TIME_API "https://worldtimeapi.org/api/ip"
#define NTP_SERVERS "0.de.pool.ntp.org", "1.de.pool.ntp.org", "2.de.pool.ntp.org"
#define NTP_MIN_VALID_EPOCH 1533081600
#define TIME_OFFSET 3600

ESP8266WebServer server(80);
// Pins
  const int pBuildinLed = LED_BUILTIN;
  const int pPumpRelais = 4;
  
struct PumpAction 
{
  int duration;
  time_t begin;
};

// Context
struct Context
{
  time_t timestamp;
  PumpAction lastPumpAction;
  int autoPumpBegin;
  int autoPumpDuration;
};
const int CONTEXT_SIZE = sizeof(Context);
const char * CONTEXT_MARKER = "WTRP";
const int CONTEXT_MARKER_SIZE = 4;
Context context;




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
    context.lastPumpAction.begin = 0;
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
    String result = "{";
      result += "\"me\" : \"" + String(hostname) + "\"";
      result += ", \"timestamp\" :" + String(context.timestamp);
      result += ", \"autoPumpBegin\" :" + String(context.autoPumpBegin);
      result += ", \"autoPumpDuration\" :" + String(context.autoPumpDuration);
      result += ", lastPumpAction :{";
        result += "\"begin\" : " + String(context.lastPumpAction.begin);
        result += ",\"duration\" : " + String(context.lastPumpAction.duration);
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

void handlePump() {
  Serial.println("Handling Pump Request"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;
   
  if (jsonInput.hasOwnProperty("duration")) { 
    int duration = (int)jsonInput["duration"];
    Serial.print("pumping ");
    Serial.println(duration);
    pumpWater(duration);
    contextSaveChanges();
    serverSendContext();
    return; 
  }
  serverSendInvalidRequest();
}

void handleScheduleSet() {
  Serial.println("Handling Schedule Set Request"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;

  bool scheduleChanged = false;
  if (jsonInput.hasOwnProperty("autoPumpBegin")) { 
    context.autoPumpBegin = (int)jsonInput["autoPumpBegin"];
    Serial.print("autoPumpBegin set to ");
    Serial.println(context.autoPumpBegin);
    scheduleChanged = true;
  }
   
  if (jsonInput.hasOwnProperty("autoPumpDuration")) { 
    context.autoPumpDuration = (int32_t)jsonInput["autoPumpDuration"];
    Serial.print("autoPumpDuration set to ");
    Serial.println(context.autoPumpDuration);
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
  server.on("/schedule/set", HTTP_POST, handleScheduleSet);

  server.onNotFound(handleNotFound);
}

// SETUP / LOOP #########################################

void pumpWater(int duration) {
  time_t now = time(nullptr);
  context.lastPumpAction.begin = now;
  context.lastPumpAction.duration = duration;
  context.timestamp = now;
  contextSaveChanges();

  digitalWrite(pPumpRelais, HIGH);
  delay(duration);
  digitalWrite(pPumpRelais, LOW);
}

void initNtp() {
  time_t n;
  configTime(TIME_OFFSET, 0, NTP_SERVERS);
  Serial.print("Wait for valid ntp response.");
  while((n = time(nullptr)) < NTP_MIN_VALID_EPOCH) {
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
  // init context
  contextInit();

  // then init context driven pins.
  pinMode(pPumpRelais, OUTPUT);
  
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
  now = DateTime(epoch);
  bool epochChanged = epoch != lastEpoch;

  // update mdns
  MDNS.update();

  // handle ota
  ArduinoOTA.handle();
  
  // handle client requests
  server.handleClient();

  if (epochChanged && epoch % 2 == 0) {
    // updateContext();
    // if (context.sunScheduleEnabled) {
    //   sunSchedule();
    // }
  }

  if (epochChanged && epoch % 60 == 0) {
    initNtp();
  }

  lastEpoch = epoch;
  delay(10);
}
#include <List.hpp>
#include <Wire.h>
#include <Arduino_JSON.h>
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
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
#define NTP_SERVERS "0.europe.pool.ntp.org", "pool.ntp.org", "time.nist.gov"
// found on https://remotemonitoringsystems.ca/time-zone-abbreviations.php
#define NTP_TIMEZONE "CET-1CEST-2,M3.5.0/02:00:00,M10.5.0/03:00:00"
#define NTP_TIMEZONE_HRS 1
int NTP_DST_HRS = 0;
#define NTP_MIN_VALID_EPOCH 1533081600

ESP8266WebServer server(80);
HTTPClient http;
// Pins
  const int pBuildinLed = LED_BUILTIN;

const int pPump1 = D5;
const int pPump2 = D6;
const int pPump3 = D7;
const int pPump4 = D0;

#define PUMP_ML_PER_MINUTE 200

struct Pump
{
  int id;
  time_t lastRun;
  int lastRunDuration;
  int autoPumpBegin;
  int duration;
  int relaisPin;
  bool isActive;
};

// Context
const int pumpCount = 4;
struct Context
{
  time_t timestamp;
  Pump pumps[pumpCount];
  char telemetryCallback[512];
};

const int CONTEXT_SIZE = sizeof(Context);
const char * CONTEXT_MARKER = "WRP2";
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
    context.pumps[0].id = 0;
    context.pumps[0].autoPumpBegin = -1;
    context.pumps[0].duration = 0;
    context.pumps[0].lastRun = 0;
    context.pumps[0].lastRunDuration = 0;
    context.pumps[0].relaisPin = pPump1;
    context.pumps[0].isActive = false;
    context.pumps[1].id = 1;
    context.pumps[1].autoPumpBegin = -1;
    context.pumps[1].duration = 0;
    context.pumps[1].lastRun = 0;
    context.pumps[1].lastRunDuration = 0;
    context.pumps[1].relaisPin = pPump2;
    context.pumps[1].isActive = false;
    context.pumps[2].id = 2;
    context.pumps[2].autoPumpBegin = -1;
    context.pumps[2].duration = 0;
    context.pumps[2].lastRun = 0;
    context.pumps[2].lastRunDuration = 0;
    context.pumps[2].relaisPin = pPump3;
    context.pumps[2].isActive = false;
    context.pumps[3].id = 3;
    context.pumps[3].autoPumpBegin = -1;
    context.pumps[3].duration = 0;
    context.pumps[3].lastRun = 0;
    context.pumps[3].lastRunDuration = 0;
    context.pumps[3].relaisPin = pPump4;
    context.pumps[3].isActive = false;
    context.timestamp = 0;
    context.telemetryCallback[0] = 0;
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

String pumpToJson(Pump pump) {
  String result = "{";
    result += "\"id\" : " + String(pump.id);
    result += ", \"autoPumpBegin\" : " + String(pump.autoPumpBegin);
    result += ", \"duration\" :" + String(pump.duration);
    result += ", \"lastRun\" :" + String(pump.lastRun);
    result += ", \"lastRunDuration\" :" + String(pump.lastRunDuration);
    result += ", \"relaisPin\" :" + String(pump.relaisPin);
    result += ", \"isPumpActive\" :" + String(pump.isActive ? "true":"false");    
  result += "}";
  return result;
}

String contextToJson() {
    String result = "{";
      result += "\"me\" : \"" + String(hostname) + "\"";
      result += ", \"timestamp\" :" + String(context.timestamp);
      result += ", \"tzHrs\" :" + String(NTP_TIMEZONE_HRS);
      result += ", \"dstHrs\" :" + String(NTP_DST_HRS);
      result += ", \"telemetryCallback\" :";
      if (context.telemetryCallback[0] == 'h') {
        result += "\"" + String(context.telemetryCallback) + "\"";
      }else{
        result += String("null");
      }       
      result += ", \"pumpMilliLiterPerMinute\" :" + String(PUMP_ML_PER_MINUTE);
      result += ", \"pumps\" :[";
      for (int i = 0; i < pumpCount; i++)
      {
        result += i == 0 ? "" : ",";
        result += pumpToJson(context.pumps[i]);
      }
      
      result += "]";
    result += "}";
    return result;
}

// SERVER I/O ###########################################
void serverSendContext() {
    String result = contextToJson();
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

void handlePumpTest() {
  Serial.println("handlePumpTest"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;

  if (!jsonInput.hasOwnProperty("id")) {
    serverSendInvalidRequest();
    return;
  }
  int pumpId = (int)jsonInput["id"];
  if (pumpId < 0 || pumpId >= pumpCount) {
    serverSendInvalidRequest();
    return;
  }
  if (!jsonInput.hasOwnProperty("duration")) { 
    serverSendInvalidRequest();
    return;
  }
  int duration = (int)jsonInput["duration"];
  if (duration <= 0  || duration >= 60 * 30) {
    serverSendInvalidRequest();
    return;
  }

  Pump* pump = &context.pumps[pumpId];
  digitalWrite(pump->relaisPin, LOW);
  delay(duration * 1000);
  digitalWrite(pump->relaisPin, HIGH);
  serverSendContext();
  return;
}

void handlePumpsSet() {
  Serial.println("handlePumpsSet");  
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput)){
    serverSendInvalidRequest();
    return;
  }
  if (!jsonInput.hasOwnProperty("pumps")){
    serverSendInvalidRequest();
    return;
  }
  JSONVar pumps = jsonInput["pumps"];
  for (int i = 0; i < pumps.length(); i++) {
    JSONVar pump = pumps[i];
    bool contextChanged = handlePumpSetJson(&pump);  
    if (!contextChanged) {
      serverSendInvalidRequest();
      return;
    }
  }
  contextSaveChanges();
  serverSendContext();
}

void handlePumpSet() {
  Serial.println("handlePumpSet"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput)) {
    serverSendInvalidRequest();
    return;
  }
  
  bool contextChanged = handlePumpSetJson(&jsonInput);  
  if (!contextChanged) {
    serverSendInvalidRequest();
    return;
  }
  contextSaveChanges();
  serverSendContext();
}

bool handlePumpSetJson(JSONVar* jsonPump) { 
  if (!jsonPump->hasOwnProperty("id")) {
    return false;
  }
  int pumpId = (int)(*jsonPump)["id"];
  if (pumpId < 0 || pumpId >= pumpCount) {
    return false;
  }

  Pump* pump = &context.pumps[pumpId];
  bool contextChanged = false;
  if (jsonPump->hasOwnProperty("autoPumpBegin")) { 
    pump->autoPumpBegin = (int)(*jsonPump)["autoPumpBegin"];
    Serial.print("autoPumpBegin set to ");
    Serial.println(pump->autoPumpBegin);
    contextChanged = true;
  }
   
  if (jsonPump->hasOwnProperty("duration")) { 
    pump->duration = (int)(*jsonPump)["duration"];
    Serial.print("duration set to ");
    Serial.println(pump->duration);
    contextChanged = true;
  }

  return contextChanged;
}

void handleSetTelemetryCallback() {
  Serial.println("handleSetTelemetryCallback"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;

  String telemetryCallback = "";
  if (jsonInput.hasOwnProperty("telemetryCallback")) { 
    telemetryCallback = (String)jsonInput["telemetryCallback"];
    telemetryCallback.toCharArray(context.telemetryCallback, 512);
  }
  
  contextSaveChanges();
  serverSendContext();
}

void sendTelemetry(Pump* pump) {
  if (context.telemetryCallback[0] != 'h') {
    return;
  }
  Serial.print("Sending telemetry to: ");
  Serial.println(context.telemetryCallback);

  String telemetry = pumpToJson(*pump);

  WiFiClient client;
  HTTPClient http;
  http.begin(client, context.telemetryCallback);
  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(telemetry.c_str());
  Serial.print("Telemetry response: ");
  Serial.println(httpResponseCode);
  // Free resources
  http.end();
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
  server.on("/telemetry/callback", HTTP_POST, handleSetTelemetryCallback);
  server.on("/pump/set", HTTP_POST, handlePumpSet);
  server.on("/pump/test", HTTP_POST, handlePumpTest);
  server.on("/pumps/set", HTTP_POST, handlePumpsSet);

  server.onNotFound(handleNotFound);
}

// SETUP / LOOP #########################################

void pumpDoStart(Pump* pump) {
  time_t now = getTimeWithDstUpdate();
  pump->isActive = true;
  pump->lastRun = now;
  pump->lastRunDuration = pump->duration;
  pumpWrite(pump, true);
}

void pumpWrite(Pump* pump, bool withTelemetry) {
  if (withTelemetry) {
    sendTelemetry(pump);
  }
  if (pump->isActive) {
    digitalWrite(pump->relaisPin, LOW);
  } else {
    digitalWrite(pump->relaisPin, HIGH);
  }
}

void pumpDoStop(Pump* pump) {
  pump->isActive = false;
  pumpWrite(pump, true);
}

void pumpSchedule(Pump* pump) {
  Serial.println("PUMP SCHEDULE ####################");
  Serial.printf("Pump Id: %d", pump->id); Serial.println();
  if (pump->autoPumpBegin < 0) {

    Serial.println("PUMP DISABLED. END.");
    return;
  }
  DateTime lastPumpBegin = DateTime(now.year(), now.month(), now.day()-1, 0, 0, 0) + pump->autoPumpBegin;
  DateTime lastPumpStop = lastPumpBegin + pump->duration;
  DateTime nowPumpBegin = DateTime(now.year(), now.month(), now.day(), 0, 0, 0) + pump->autoPumpBegin;
  DateTime nowPumpStop = nowPumpBegin + pump->duration;
  char buff[20];
  Serial.printf("NOW: %s", now.tostr(buff)); Serial.println();
  Serial.printf("Last Pump Begin: %s", lastPumpBegin.tostr(buff)); Serial.println();
  Serial.printf("Last Pump Stop: %s", lastPumpStop.tostr(buff)); Serial.println();
  DateTime pumpBegin, pumpStop;
  if(lastPumpStop > now) {
    pumpBegin = lastPumpBegin;
    pumpStop = lastPumpStop;
  } else {
    pumpBegin = nowPumpBegin;
    pumpStop = nowPumpStop;
  }
  Serial.printf("Picked Pump Begin: %s", pumpBegin.tostr(buff)); Serial.println();
  Serial.printf("Picked Pump Stop: %s", pumpStop.tostr(buff)); Serial.println();
  bool targetPumpState = now > pumpBegin && now <= pumpStop;
  Serial.printf("targetPumpState = %s", targetPumpState ? "ON" : "OFF"); Serial.println();
  Serial.printf("currentPumpState = %s", pump->isActive ? "ON" : "OFF"); Serial.println();

  if (targetPumpState != pump->isActive) {
    pump->isActive = targetPumpState;
    Serial.printf("Pump State switched to %s", pump->isActive ? "ON" : "OFF"); Serial.println();
    if (targetPumpState) {
      pumpDoStart(pump);
    } else {
      pumpDoStop(pump);
    }
    contextSaveChanges();
  }
  Serial.println("#################################");
}

void initNtp() {
  configTzTime(NTP_TIMEZONE, NTP_SERVERS);
  Serial.print("Wait for valid ntp response.");
  getTimeWithDstUpdate();  
  Serial.println();
}

time_t getTimeWithDstUpdate() {
  struct tm timeinfo = {0};
  while (!getLocalTime(&timeinfo, 0)) {  // wait for NTP to sync
    delay(500);
  }
  const uint16_t daysInMonth[12] = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
  time_t seconds = 0;
  uint16_t year;
  uint8_t month;

  for (year = 1970; year < timeinfo.tm_year + 1900; year++) {
    for (month = 0; month < 12; month++) {
      seconds += daysInMonth[month] * 86400UL;

      // Adjust for leap years
      if (month == 1 && ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)) {
        seconds += 86400UL;
      }
    }
  }

  for (month = 0; month < timeinfo.tm_mon; month++) {
    seconds += daysInMonth[month] * 86400UL;
      
    // Adjust for leap years
    if (month == 1 && ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)) {
      seconds += 86400UL;
    }
  }

  seconds += (timeinfo.tm_mday - 1) * 86400UL;
  seconds += timeinfo.tm_hour * 3600UL;
  seconds += timeinfo.tm_min * 60UL;
  seconds += timeinfo.tm_sec;
    
  return seconds;
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
  ArduinoOTA.setHostname(hostname);
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

  for (int i = 0; i < pumpCount; i++)
  {
    pinMode(context.pumps[i].relaisPin, OUTPUT);
    pumpWrite(&context.pumps[i], false);
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
  time_t epoch = getTimeWithDstUpdate();
  now = DateTime(epoch);
  bool epochChanged = epoch != lastEpoch;

  // update mdns
  MDNS.update();

  // handle ota
  ArduinoOTA.handle();
  
  // handle client requests
  server.handleClient();

  if (epochChanged) {
    context.timestamp = epoch;
    for (int i = 0; i < pumpCount; i++) {
      pumpSchedule(&context.pumps[i]);
    }
  }
  

  if (epochChanged && epoch % 60 == 0) {
    initNtp();
  }

  lastEpoch = epoch;
  delay(10);
}
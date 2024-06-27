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

const int pPump1 = D5;
const int pPump2 = D6;
const int pPump3 = D7;
const int pPump4 = D0;


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

String pumpToJson(Pump pump)
{
  String result = "{";
    result += "\"id\" : " + String(pump.id);
    result += ", \"autoPumpBegin\" : " + String(pump.autoPumpBegin);
    result += ", \"duration\" :" + String(pump.duration);
    result += ", \"lastRun\" :" + String(pump.lastRun);
    result += ", \"lastRunDuration\" :" + String(pump.lastRunDuration);
    result += ", \"relaisPin\" :" + String(pump.relaisPin);
  result += "}";
  return result;
}

// SERVER I/O ###########################################
void serverSendContext() {
    String result = "{";
      result += "\"me\" : \"" + String(hostname) + "\"";
      result += ", \"timestamp\" :" + String(context.timestamp);
      result += ", \"pumps\" :[";
      for (int i = 0; i < pumpCount; i++)
      {
        result += i == 0 ? "" : ",";
        result += pumpToJson(context.pumps[i]);
      }
      
      result += "]";
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

  Pump* pump = &context.pumps[pumpId];
  bool scheduleChanged = false;   
  if (jsonInput.hasOwnProperty("duration")) { 
    pump->duration = (int)jsonInput["duration"];
    Serial.print("duration set to ");
    Serial.println(pump->duration);
    scheduleChanged = true;
  }

  if (scheduleChanged) {
    pumpDoStart(pump);
    delay(pump->duration * 1000);
    pumpDoStop(pump);
    serverSendContext();
    return;
  }
  serverSendInvalidRequest();
}

void handlePumpTestAll() {
  Serial.println("handlePumpTestAll"); 
  JSONVar jsonInput;
  if (!serverParseJson(&jsonInput))
    return;

  int pumpDuration = 0;
  if (jsonInput.hasOwnProperty("duration")) { 
    pumpDuration = (int)jsonInput["duration"];
  }

  if (pumpDuration > 0) {
                    {
      Pump* pump = &context.pumps[i];
      pump->duration = pumpDuration;
      pumpDoStart(pump);
    }
    delay(pumpDuration * 1000);
    for (int i = 0; i < pumpCount; i++)
    {
      Pump* pump = &context.pumps[i];
      pump->duration = pumpDuration;
      pumpDoStart(pump);
    }
    serverSendContext();
    return;
  }
  serverSendInvalidRequest();
}

void handlePumpSet() {
  Serial.println("Handling Schedule Set Request"); 
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

  Pump* pump = &context.pumps[pumpId];
  bool scheduleChanged = false;
  if (jsonInput.hasOwnProperty("autoPumpBegin")) { 
    pump->autoPumpBegin = (int)jsonInput["autoPumpBegin"];
    Serial.print("autoPumpBegin set to ");
    Serial.println(pump->autoPumpBegin);
    scheduleChanged = true;
  }
   
  if (jsonInput.hasOwnProperty("duration")) { 
    pump->duration = (int)jsonInput["duration"];
    Serial.print("duration set to ");
    Serial.println(pump->duration);
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
  server.on("/pump/set", HTTP_POST, handlePumpSet);
  server.on("/pump/test", HTTP_POST, handlePumpTest);
  server.on("/pump/test/all", HTTP_POST, handlePumpTestAll);

  server.onNotFound(handleNotFound);
}

// SETUP / LOOP #########################################

void pumpDoStart(Pump* pump) {
  time_t now = time(nullptr);
  pump->isActive = true;
  pump->lastRun = now;
  pump->lastRunDuration = pump->duration;
  pumpWrite(pump);
}

void pumpWrite(Pump* pump) {
  if (pump->isActive) {
    digitalWrite(pump->relaisPin, LOW);
  } else {
    digitalWrite(pump->relaisPin, HIGH);
  }
}

void pumpDoStop(Pump* pump) {
  pump->isActive = false;
  pumpWrite(pump);
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

  for (int i = 0; i < pumpCount; i++)
  {

    pinMode(context.pumps[i].relaisPin, OUTPUT);
    pumpWrite(&context.pumps[i]);
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
  time_t epoch = time(nullptr);
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
#include <Wire.h>
#include <Arduino_JSON.h>
#include <ESP8266WiFi.h>
#include <WiFiClient.h>
#include <ESP8266WebServer.h>
#include <ESP8266mDNS.h>
#include <EEPROM.h>

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

ESP8266WebServer server(80);

// Pins
  const int pBuildinLed = LED_BUILTIN;

  const int pFan = D0;
  // TODO: BME280!, Supersonic? (plant height), Soil moisture?, EC / PH?, 
  // pumps/fert/water?, 


// Context
struct Context
{
  // Fan Speed, 255 = OFF, 0 = MAX.
  unsigned char fanSpeed;
};
int CONTEXT_SIZE = sizeof(Context);
const char * CONTEXT_MARKER = "GROW";
int CONTEXT_MARKER_SIZE = 4;
Context context;

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
    String result = String("{");
    result += "\"fanSpeed\":" + String(context.fanSpeed);
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

void configureRoutes() {
  server.on("/", handleRoot);
  server.on("/fan/set", HTTP_POST, handleFanSet);
  server.onNotFound(handleNotFound);
}

// SETUP / LOOP #########################################

// blink with delay
void blink(unsigned int ms)
{
  unsigned int half = ms / 2;

  digitalWrite(pBuildinLed, LOW);
  delay(half);
  digitalWrite(pBuildinLed, HIGH);
  delay(half);
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
  pinMode(pFan, OUTPUT);
  analogWrite(pFan, context.fanSpeed);
  
  // connect wifi
  Serial.println("Connecting");
  WiFi.hostname(hostname);
  WiFi.begin(ssid, pass);

  // Wait for connection
  while (WiFi.status() != WL_CONNECTED) {
    blink(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.print("Connected to ");
  Serial.println(ssid);
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
  
  // AutoReconnect
  WiFi.setAutoReconnect(true);
  WiFi.persistent(true);
  
  // start mdns responder hostname is better than ip ;)
  if (MDNS.begin(hostname)) 
  { 
    Serial.println("MDNS responder started"); 
  }

  // configure routes
  configureRoutes();

  // launch server.
  server.begin();
  Serial.println("HTTP server started");
  blink(2000);
}

void loop() {
  // update mdns
  MDNS.update();
  
  // handle client requests
  server.handleClient();
}
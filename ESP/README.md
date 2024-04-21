<div align="center">
    <img src="./res/logo.png" width="400" />
</div>

# GrowBox
Model: <a target="_blank" href="https://amzn.to/3x9uhFN">MARS HYDRO TS1000 Growbox Kit</a>

This project aims to Automate GrowBoxes in order to make the freshes and biggest fruits / flowers of your plants. 
#### Working Features
 - WebClient on ESP.
 - Inline Fan Control for the analog fan.
 - Humidity / Temperature / Pressure <a target="_blank" href="https://amzn.to/3TYbDtB">GY-BME280</a>.

#### Planned Features
 - Plant height, Supersonic.
 - Pump control for fertilizer, water. 
 - Water warning, via soil moisture? _Unclear if this can't be just calculated way better than probed._
 - EC / PH, _as the sensors are quite expensive i might skip this try or push it way back the road._


## Features
### WebClient
<img src="./res/client.png" width="200" />

WebClient control fan speed and read current or retained conditions obtained by the <a target="_blank" href="https://amzn.to/3TYbDtB">GY-BME280</a>.

### API
API definition of the GrowBox, API is using JSON for data and plain text for errors. 

<hr/>

#### Get
Returns the current context. 
|Context||
|-|-|
|me|hostname|
|fsMounted|file system mounted 1 / 0|
|fanSpeed|the fan speed, 255 = OFF, 0 = MAX|
|bme|current bme data|
|bmeRetained|retained bme readings|

![GET](https://img.shields.io/badge/GET-blue)<br/>
`http://growbox01/get`
#### Response 
![200](https://img.shields.io/badge/200-green)
```json
{
    "me": "GrowBox01",
    "fsMounted": "1",
    "fanSpeed": 255,
    "bme": {
        "temperature": 20.80,
        "humidity": 45.48,
        "pressure": 99961.20,
        "timestamp": 1712060127
    },
    "bmeRetained": [
        {
            "temperature": 20.83,
            "humidity": 44.85,
            "pressure": 99960.22,
            "timestamp": 1712057103
        },
        {
            "temperature": 20.85,
            "humidity": 45.49,
            "pressure": 99970.94,
            "timestamp": 1712053501
        },
        {
            "temperature": 20.90,
            "humidity": 47.11,
            "pressure": 99957.71,
            "timestamp": 1712049899
        }
    ]
}
```
<hr/>

#### Fan Set
Sets the fan speed, 255 = OFF, 0 = MAX. Fanspeed is stored in EEPROM / Context.

![POST](https://img.shields.io/badge/POST-green)<br/>
`http://growbox01/fan/set`
#### Request
```json
{
    "fanSpeed": 150
}
```
#### Response 
![200](https://img.shields.io/badge/200-green) ![400](https://img.shields.io/badge/400-red)

The current context.

```json
{
    "me": "GrowBox01",
    "fsMounted": "1",
    "fanSpeed": 255,
    "bme": {
        "temperature": 20.80,
        "humidity": 45.48,
        "pressure": 99961.20,
        "timestamp": 1712060127
    },
    "bmeRetained": [
        {
            "temperature": 20.83,
            "humidity": 44.85,
            "pressure": 99960.22,
            "timestamp": 1712057103
        },
        {
            "temperature": 20.85,
            "humidity": 45.49,
            "pressure": 99970.94,
            "timestamp": 1712053501
        },
        {
            "temperature": 20.90,
            "humidity": 47.11,
            "pressure": 99957.71,
            "timestamp": 1712049899
        }
    ]
}
```

## D1 Mini Board Config
<a href="./res/arduino.json">arduino.json</a> or:
<div align="center">
    <img src="./res/board-config.png" width="400" />
</div>

## Inline Fan Fritzing
100Ohm Resistor, BC337 Transistor, D1 Mini, 24v Stepdown.
If you dissassemble the Fan you will find GND, VCC, R+ R- as pins on the driver board. CAUTION 220v are transformed on the same board, be aware of it. The R+ and R- pins are connected to the control knob cable, I reused this.
<a href="./InlineFan.fzz">
    <img src="./res/InlineFan_Steckplatine.png">
</a>

### Amazon Basket

|Item|Description|
|-|-|
| <a target="_blank" href="https://amzn.to/3TDTPma">ESP-8622 D1 Mini</a> |  D1 Mini to operate the Web-API. |
| <a target="_blank" href="https://amzn.to/4avrZPI">Transistor BC337</a> |  A BC337 transistor.  |
| <a target="_blank" href="https://amzn.to/4aB79hM">100Ohm Resistor</a> |  A 100Ohm resistor.  |
| <a target="_blank" href="https://amzn.to/3VyUdVE">Some cables</a> |  I always take silicone wrapped cables because of their heat resistance and general durability.  
| <a target="_blank" href="https://amzn.to/3J1IyHe">24v Stepdown</a> | Any stepdown converter from 24v to 5v. |
| <a target="_blank" href="https://amzn.to/3TYbDtB">GY-BME280</a> | GY-BME280 sensor for humidity / temperature / pressure. |




_Those are affiliate links, use them if you want to support me :)_
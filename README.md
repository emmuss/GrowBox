<div align="center">
    <img src="./res/logo.png" width="400" />
</div>

# GrowBox
Model: <a target="_blank" href="https://www.amazon.de/gp/search?ie=UTF8&tag=emmuss-21&linkCode=ur2&linkId=709ebbc777fc71d4af2c47dd75e933fe&camp=1638&creative=6742&index=computers&keywords=MARS HYDRO TS1000 Growbox Kit">MARS HYDRO TS1000 Growbox Kit</a>

This project aims to Automate GrowBoxes in order to make the freshes and biggest fruits / flowers of your plants. 
#### Working Features
 - Inline Fan Control for the analog fan.

#### Planned Features
 - Humidity / Temperature / Pressure GY-BME280.
 - Plant height, Supersonic.
 - Pump control for fertilizer, water. 
 - Water warning, via soil moisture? _Unclear if this can't be just calculated way better than probed._
 - EC / PH, _as the sensors are quite expensive i might skip this try or push it way back the road._


## Features
### API
API definition of the GrowBox, API is using JSON for data and plain text for errors. 

<hr/>

#### Index / Root
Returns the current context. 

![POST](https://img.shields.io/badge/GET-blue)<br/>
`http://growbox01/`
#### Response 
![200](https://img.shields.io/badge/200-green)
```json
{
    "fanSpeed": 150
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
    "fanSpeed": 150
}
```

## D1 Mini Board Config
<a href="./res/arduino.json">arduino.json</a> or:
<div align="center">
    <img src="./res/board-config.png" width="400" />
</div>

### Amazon Basket

|Item|Description|
|-|-|
| <a target="_blank" href="https://www.amazon.de/gp/search?ie=UTF8&tag=emmuss-21&linkCode=ur2&linkId=c248904b195458de5fac95c309a80db7&camp=1638&creative=6742&index=computers&keywords=AZ-Delivery D1 Mini">ESP-8622 D1 Mini</a> |  D1 Mini to operate the Web-API. |
| <a target="_blank" href="https://www.amazon.de/gp/search?ie=UTF8&tag=emmuss-21&linkCode=ur2&linkId=b5f99c86871cf499b06743e0fd7af662&camp=1638&creative=6742&index=computers&keywords=Transistor Kit BC337">Transistor BC337</a> |  A BC337 transistor.  |
| <a target="_blank" href="https://www.amazon.de/gp/search?ie=UTF8&tag=emmuss-21&linkCode=ur2&linkId=88a59624d857f796cf99519988729319&camp=1638&creative=6742&index=computers&keywords=Resistor Kit 100Ohm">100Ohm Resistor</a> |  A 100Ohm resistor.  |
| <a target="_blank" href="https://www.amazon.de/gp/search?ie=UTF8&tag=emmuss-21&linkCode=ur2&linkId=d2b48a684c699222c0691b4811bbffe6&camp=1638&creative=6742&index=computers&keywords=Silicon Cableset 30AWG">Some cables</a> |  I always take silicone wrapped cables because of their heat resistance and general durability.  |

_Those are affiliate links, use them if you want to support me :)_
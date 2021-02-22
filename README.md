# Buderus KM200 Reader

![Build + Test](https://github.com/PeterPuff/buderus-km200-reader/workflows/Build%20+%20Test/badge.svg)
[![codecov](https://codecov.io/gh/PeterPuff/buderus-km200-reader/branch/master/graph/badge.svg?token=IE5H4BOX7Q)](https://codecov.io/gh/PeterPuff/buderus-km200-reader)

## Description

Buderus KM200 Reader provides functionality to read datapoint data from Buderus KM200 communication module.

The base functionality was translated from demel42's IP-Symcon module https://github.com/demel42/IPSymconBuderusKM200/. Further  description of supported devices can also be found there.

To translate decryption algorithm, this source was used: https://stackoverflow.com/questions/19719294/decrypt-string-in-c-sharp-that-was-encrypted-with-php-openssl-encrypt.

## Tested hardware

This library was tested with a Buderus heat pump heating system (indoor unit IDU-8), equipped with following modules:
- Buderus Logamatic HMC300
  - Software version NF13.06
- "Internet module"
    - Internal gateway module, included in tower, no specific name
    - Details shown in menu of main unit: 
        - Internet module: *KMx*
        - Software version *04.07*
    - Details read from the interface via it's datapoints
        - Hardware version: *iCom_Low_NSC_v1* (`/gateway/versionHardware`)
        - Software version: *04.07.05* (`/gateway/versionSoftware`)

## Sample response from device

Query URL: http://device-address-and-port/system/sensors/temperatures/outdoor_t1

Decrypted response: 
```json
{
    "id": "/system/sensors/temperatures/outdoor_t1",
    "type": "floatValue",
    "writeable": 0,
    "recordable": 1,
    "value": 0.6,
    "unitOfMeasure": "C",
    "state": [
        {
            "open": -3276.8
        },
        {
            "short": 3276.7
        }
    ]
}
```

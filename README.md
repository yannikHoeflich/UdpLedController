# Udp Led Controller
This is a led controller (mainly for led strips) which controlles the leds over udp and provides an api with a basic website to switch animations / colors

# Getting started
## Micro Controller
go /MicroController directory in the repository and check if your controller with the correct led protocol has an existing script

### a script already exists
1. clone the repository or only the script
2. insert in the #define parameters your parameters
   - STASSID: wifi name
   - STAPSK : wifi password
   - NUM_LEDS: the amount of leds that are on your product
   - DATA_PIN: the pin that your device is connected with
3. deploy script to the microcontroller

### write your own script
the script have to match to the current requirements
- connect to the same network as your controller
- open an udp socket
  - port 4210
  - on connection
    - first byte is 0: send back the amount of leds (16 bit integer)
    - first byte is not 0: all other bytes are colors (rgb, 3 bytes per color)

you can create a can make a merge request so I can add the script to the repository

## Http Controller

### download
download the newest version for your os in [releases](https://github.com/yannikHoeflich/UdpLedController/tags)

### or compile it yourself
1. download [.NET core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
2. compile with "dotnet build --runtime [RID](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) --configuration Release"

### run
run the program with higher privileges (windows: admin, linux: root)

### install animations
1. use the command `install` while controller runs
    - usage `install [json download url \ animation name(uses the `[animation repository](https://github.com/yannikHoeflich/LedAnimations)`)]`
    - the command can also be used as start argument `./Controller install [url] (/s)`
      - url: json download url \ animation name(uses the [animation repository](https://github.com/yannikHoeflich/LedAnimations))
      - /s: starts the programm anyways
    - example: `install all`
2. install by hand
   1. copy directory /Controller/data from repository in the same directory as your program
   2. click "reload animations" button an website 'http://[ip of device]/'

### use
1. go on the website 'http://[ip of device]/'
2. click on "scan for devices" to scan your network for matching udp sockets
3. if not all animations are visible click on "reload Animations"
4. click on any animation to run that animation

# Documentations
[api](/Documentations/api_documentation.md)  
[animation scripts](/Documentations/animation_script_documentation.md)  
[animation metadata json](/Documentations/animation_metadata_json.md)

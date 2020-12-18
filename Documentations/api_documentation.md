# API Documentation

## request
endpoint: `/api?method=[method]`
### Get Animations
gets an array of all animations
method:  
`getAnimations`  

parameters:

returns:  
- `Animation` `array`

### Get Devices
gets an array of all devices
method:  
`getDevices`  

parameters:

returns:  
- `Device` `array`

### Set Animation
sets an animation, which is running after the call
method:  
`setAnimation`  

parameters:
- `ulong id` => index of the animation in the array

returns:  
- `boolean` => if animation set was succesful

### Get current Animation
gets the current running animation
method:  
`getCurrentAnimation`  

returns:  
- `Animation`
  
### scan Devices
scan for devices in the local network
method:  
`scanDevices`  

returns:  
- `Device` `array`

### scan Devices
scan for devices in the local network
method:  
`scanDevices`  

parameters:
- (`string ip` => an example ip in the network only nessesary of the controller is in multiple networks  )

returns:  
- `Device` `array`

### scan Animations
reloads all animations from animation file
method:  
`scanAnimations`  

returns:  
- `boolean` => if reload set was succesful
 

## object types

### Animation
Fields
- `string Name` => name of the animation
- `bool IsAnimated` => boolean if animation is animated or not

### Device
Fields
- `string Ip` => local ip adress of device
- `Color` `array` `Leds` => current led collors
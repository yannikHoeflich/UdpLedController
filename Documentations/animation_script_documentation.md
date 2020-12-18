# animation script Documentation

[examples](https://github.com/yannikHoeflich/LedAnimations/tree/master/InstallationScripts)

## Properties
- int LedLength: the sum of all leds from all devices
- Time: the current time, updated every run
  - int Hour: 
  - int Minute: 
  - int Second: 

## Functions

### sleep(milliseconds)
wait a timespan  
parameters:
 - `int milliseconds`: the time too wait in milliseconds

### SetColor(int index, Color color)
sets the color of a specific led (doesn't update)  
parameters:
 - `int index`: the index of the led that gets set
 - `Color color`: the color the led gets

### Update()
updates the leds of all devices

### SetColor(Object object)
Converts an object to string and prints it into the console  
parameters:
 - `Object object`: tthe object that get converted

### Random(int min, int max)
generates a random number between the min and max value  
parameters:
 - `int min`: the min value that can get generated
 - `int max`: the max number, the result is always lower
returns:
- `int`: the random number

### Round(double value)
rounds the value to a int
parameters:
 - `double value`: the value that gets rounded
returns:
- `int`: the rounded number

### RunExternal(string name)
runs another animation by its name (don't switch just run it **before** the leds gets auto updated)
parameters:
 - `string name`: the animation name that gets runned ones

## Classes
### Color
the default `System.Drawing.Color` class in .NET core 3.1 
static methods:
- `FromArgb([int a], int r, int g, int b)`: converts (alpha) red, green, and blue value to an instance

properties:
- `int R`: red value
- `int G`: green value
- `int B`: blue value

[more details](https://docs.microsoft.com/de-de/dotnet/api/system.drawing.color?view=net-5.0)


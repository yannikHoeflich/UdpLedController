# animation script Documentation

[examples](https://github.com/yannikHoeflich/LedAnimations/tree/master/InstallationManifests)

## Properties
- `string[] dependencies`: urls to json files of other animations
- `string name`: the name of the animation (can get changed with data values with `%data key%`)
- `string author`: the author name (gets added to the name like author.name)
- `string downloadUrl`: the url of the JavaScript file
- `string version`: the version of the animation (not implimented yet)
- `string scriptPath`: gets set later, only nessesary to impliment by hand
- `bool isAnimated`: if the animation needs to get runned more than ones
- `int delay`: the delay between the updates
- `bool hidden`: if the animation can get set by the api (if false only by RunExternal)
- `Data[] data`: gets set later, only nessesary to impliment by hand

## Objects
### Data
Properties:
- `string name`: name of data key
- `dynamic value`: data

special names:
- `_type_[name]`: sets the type of another parameter to a c# type examples:
  - `System.Drawing.Color` to save a color as parameter (color have to be like: `string: "r, g, b"`)
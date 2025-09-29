
# `.lib` folder

This is the location of dll files referenced during development.

The file/folder structure should be as follows:

```
root/
  ├ Managed/
  │    ├ Cinemachine.dll
  │    ├ InControl.dll
  │    ...  (copy from "$(GamePath)\A Short Hike_Data\Managed" as needed)
  ├ preloads/  (output directory of preload dll when using "Build.Preload.xml")
  │    ├ *.preload.dll
  ├ Assembly-CSharp.dll  (copy from "$(GamePath)\A Short Hike_Data\Managed")
  ├ ModdingAPI.dll
  └ ModdingAPI.xml
```

Referenced from:

- `/Directory.Build.targets`
- `/Build.Preload.xml`
- `/ModdingAPI/Directory.Build.targets`

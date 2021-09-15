# AnimalAI

Animal-AI-Version3-Dev

Unity version:      2020.3.9f1
ML-Agents version:  2.0.0 (0.26.0)


## Getting Started

Make sure you have the correct Unity version installed on your system. Starting Unity in the project folder, it will automatically install the other required packages (e.g. Unity ML-Agents).

From the Unity Editor, you can start playing with the AnimalAI arena by opening the `AAI3EnvironmentManager` Scene and pressing the `Play` button in the Editor.

By default, the arena will be randomly generated. If you want to use a preconfigured arena, make sure the correspondent configuration file is placed under the `Resources` folder. Then, select the `EnvironmentManager` GameObject and indicate the path of your config file in the `Config File` script variable field, using a path that is relative to the `Resources` folder and omitting the file format, e.g. `competition/1-1-1`.

## Adding new items to the arena

New items models should be added under the `Prefabs` folder. After having created a new item, you need to make sure it is also available as a Prefab for the AAI3Arena.

To do so, open the `AAI3Arena` Prefab under Prefabs and add your new item to the `Prefabs` list under `All Prefabs`, in the Training Arena Script section.

Now you are all set to start using your new item!

Add it to an arena configuration file, like this:

     - !Item
      name: <YourNewItemName>
      ....

replacing `<YourNewItemName>` with the name you gave to your Prefab and try it out.

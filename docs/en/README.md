---
description: Work without Augmenta hardware by simulating people moving in space
---

# Simulator

### Augmenta-Simulator

Work without Augmenta hardware by simulating people moving in space.

![](https://media.giphy.com/media/lRdaGBXEvU5mAj0z5K/giphy.gif)

## Download

{% hint style="info" %}
👋 Download Simulator for Windows & MacOS [here](https://augmenta.tech/augmenta-simulator-download/)
{% endhint %}

## Limitations

Here are the differences that you can expect between the simulator and real-world data:

- The data of people moving around will not be as linear as the objects moving with the default settings of the simulator. There are a few filters to generate variations in speed, orientation etc to get closer to real-world data that you can use to better test your content.
- In the simulated data provided by this tool, the `centroid` data will always be at the exact same place than the `bounding box center` of each object. This will not be the case in real-world data, as the centroid is the *center of mass* of each tracked object and the *boudning box center* is used to place the bounding box at the right position in space. For example, if you are standing up and extending an arm to your side, the *bounding box center* will be close to your extended arm's shoulder (halfway between the tip of your hand and your opposite shoulder). But the *centroid* will be closer to your chest, because when the detection and tracking is done, the camera will see more points belonging to your chest/head/center of your body than points belonging to your arms.
- There is no support for OSC bundles in the TUIO output of the Augmenta Simulator. If you encounter performance issues on the creative software side with a lot of points in TUIO, it might come from too much messages coming in. There is an option to bundle the updates together in Augmenta Fusion that can fix such issues. 

## Controls

### Application Shortcuts

Left click : Create point or move existing point

Right click : Delete point

M : Mute/Unmute all outputs

H : Toggle UI

F1 : Toggle Help

### Camera Control

Use mousewheel to zoom.

Press and drag mousewheel to pan.

Press Alt + Left clic to rotate.

Press R to reset camera position.

Press P to switch camera projection between orthographic (iso) or perspective (pers).

## Zeroconf

For the simulator to create a Zeroconf service on Windows, you need to have Apple Bonjour for Windows installed.

## Version

Unity 2021.2.17f1

# Mission Control (MRI VR)
**Project Links:**  
[GitHub Repository](https://github.com/CRISPGroup/Mission-Control-MRI-VR/) | [OSF Project](https://doi.org/10.17605/OSF.IO/NT4GX) | [Preprint]()
<div align="center">
    <img height="320px" src="https://github.com/J-Scan/MRIExperience/blob/main/Submission/mission-control-logo.png">
    <img height="320px" src="https://github.com/J-Scan/MRIExperience/blob/main/Submission/MRIVR.png">
</div>

## Trailer Video
https://youtu.be/6e5fm8P5fFA

## Project description
Children undergoing MRI scans can often feel anxious and restless, leading to movement artifacts and the potential need for sedation during the procedure. To enhance patient experience and reduce the need for sedation, we created Mission Control, an immersive VR simulation that simulates a rocket landing to prepare children for the MRI process. After familiarizing themselves with the MRI room and scanner, children board their spaceship and observe the launch and landing through a window. To successfully land, they must focus on the target, keep their head still, and follow visual and audio cues to minimize head movement and safely reach the moon.

Note: This is the updated version of the [previous repository](https://github.com/J-Scan/MRIExperience), which contains the early versions of Mission Control.

## Technologies/features used
We created a Virtual Reality environment with the Unity Game Engine designed for the Oculus Quest 2, Quest 3 (HTC, Pico, etc., need to be tested but they should be compatible as we used OpenXR).

## Assets/dependencies
While the source code is available here, most of the 3D models, textures, and materials have been omitted in order to comply with the licenses and usage rights granted at the time of purchase. As a result, this repository is intended primarily as an example to demonstrate the structure, logic, and implementation approach rather than a fully functional, ready-to-run Unity project. The resulting build is still available free to use.

## Installation

To open this Unity project, clone or download this repository and open the folder with **Unity 6000.2.2f1**.

You now have access to the source code in the various subfolders of the project (for example, in `Assets/Scenes/*/Scripts`) and to the audio files in `Assets/Scenes/*/Audio`.  
You can also explore the `EventSequence` scripts in the **Hierarchy** to understand how the events are structured. If you need more information, consider checking our [preprint]().

Please note that since most 3D models, textures, and materials have been omitted to comply with the licenses purchased, pressing the **Play** button may not work as expected. This repository is therefore intended primarily as a **reference and study resource** rather than a fully functional project. However, the full resulting Mission Control application is provided and free to use.

---

To install the **Mission Control** application on your VR headset:

1. Download the APK [here](LINK).
2. Enable **Developer Mode** on your Meta account.
3. Transfer the APK to your headset using tools such as **Meta Developer Hub**, **ADB**, or any similar method.
4. On your headset, go to the **Library** and launch the app from the **Unknown Sources** tab.

We hope to make this process easier in the future by publishing the application directly on the **Meta Store**.

If either you want to mirror the VR view, please use this [script](https://github.com/carodak/missioncontrol-view-mirrorer).

## API Documentation

The API Documentation will be available soon.

## Authors

Dakoure, Caroline[1], Sousa,  Ana Elisa [1], Sanches, Liana [1,2], Sung Justin [3], Catwell Naomi[4], Valiquette Vanessa [1], Lepage Martin [1,2,5]

[1] Douglas Research Centre, Montreal, QC, Canada  
[2] Department of Psychiatry, McGill University, Montreal, QC, Canada  
[3] Integrated Program in Neuroscience, McGill University, Montreal, QC, Canada  
[4] Département de génie logiciel et des TI, École de technologie supérieure, QC, Canada  
[5] Douglas Mental Health University Institute, Montreal, QC, Canada

---

### Credits for free assets
Office chair: https://sketchfab.com/3d-models/office-chair-654794ae5bb44303a3fa5223f552f4e4
Recycle bin: https://sketchfab.com/3d-models/recycle-bin-1128266865624dcf96a34bde46185d42


## License

This project is licensed under the **MIT License**, see the [LICENSE](./LICENSE.md) file for details.


## References/resources:

Brown RKJ, Petty S, O'Malley S, Stojanovska J, Davenport MS, Kazerooni EA, Fessahazion D. Virtual Reality Tool Simulates MRI Experience. Tomography. 2018 Sep;4(3):95-98. doi: 10.18383/j.tom.2018.00023. PMID: 30320208; PMCID: PMC6173786.

Le May S, Genest C, Hung N, Francoeur M, Guingo E, Paquette J, Fortin O, Guay S. The Efficacy of Virtual Reality Game Preparation for Children Scheduled for Magnetic Resonance Imaging Procedures (IMAGINE): Protocol for a Randomized Controlled Trial. JMIR Res Protoc. 2022 Jun 13;11(6):e30616. doi: 10.2196/30616. PMID: 35700000; PMCID: PMC9237773.

Nakarada-Kordic I, Reay S, Bennett G, Kruse J, Lydon AM, Sim J. Can virtual reality simulation prepare patients for an MRI experience? Radiography (Lond). 2020 Aug;26(3):205-213. doi: 10.1016/j.radi.2019.11.004. Epub 2019 Nov 28. PMID: 32052767.

## Contact
caroline.dakoure@umontreal.ca
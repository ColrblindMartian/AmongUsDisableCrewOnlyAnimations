# Among Us Disable Crew Only Animations
For the Game: Among Us. Disables the animations that can only be done by a crewmember and not by an impostor



<h1>INFO</h1>
<b>I am not the creator of this file, I only modified it to disable a few animations stated further below. All credit for this file and the awesome game goes to the publisher and developer of this game: Innersloth http://innersloth.com</b><br>

<b>These changes will only affect your game! If you want to have a fair match with this mod then you have to make sure everybody in the game uses this modification.</b>
Shouldn't be that hard if you play with the same guys or with a group of friends.

<h1>Introduction</h1>
Hi fellow Crewmates,
I've been playing this game for a few weeks now and I think it is pretty fun.

There are just some gameplay mechanics that I think could be improved and would make the game more thrilling.

In particular, I don't like the fact that there are certain tasks that can verify a crewmate and prove that he/she is not an impostor. I think this takes some excitement out of the game.
That's why I tried to mod the game and disable those animations for you.

<h1>HowTo</h1>
Tested with Version v2020.5.9s

All you have to do is download the modded asset file and replace it with the file at your game location<b>(create a backup of the file first)</b>.<br>
File: sharedassets0.assets (https://github.com/marsmann007/AmongUsDisableCrewOnlyAnimations/raw/master/sharedassets0.assets)
Replace with file in: C:/Program Files/Steam/steamapps/common/Among Us/Among Us_Data/
<br><br>
To undo these changes all you have to do is replace the sharedassets0.assets file with the backup file you created earlier and restart the game.
<br>
<h1>Affected Animations</h1>
Following Tasks will be affected (all maps):
<ul>
  <li>Medbay scan (Submit scan)</li>
  <li>Clear Asteroids (Weapons firing)</li>
  <li>Prime Shields (Map: THE SKELD: lights indicating shield on/off will always be off)</li>
  <li>Empty Garbage (Map: THE SKELD: remove garbage exiting the ship hatch)</li>
</ul><br>
<h3>Let me know if I missed an animation.</h3>

<h1>How did I do it?</h1>
For those interested in what changes I had to make to disable the animations..
<br><br>
Every animation in this game (except for the garbage and leaf exiting the hatch animation, more on that later) consists of a specific amount of frames. All I did was to replace the linked frames in the animation with the "idle" frame, the animations are still playing but all frames are the same and thus it looks like the animations are turned off.
<br><br>
I used a Hex editor to make the changes.<br>
It took me some time to find the locations in the file, but with the knowledge on how many frames each animation has and by a lot of trial and error I made it.
<br><br>
The garbage exiting the hatch is a completely different story and took me as much if not even more time than all the other animations together.<br>
The difference is that the other animations are pre-defined/-drawn animations consisting of a defined amount of frames.
The garbage exiting the hatch however is a particle effect. This means it will eject a random amount of the garbage (leaves, garbage, totem etc.) with a random velocity out of the hatch of the ship.<br>
First I tried to "delete" the garbage images, but the problem was that there is another task (clean O2 filter) that needs the images of the leaves in order to work.<br>
My final solution was to disable the particle system renderer for this hatch. Disabling it is easy all I had to do is switch a flag from 1 to 0, the real problem was finding the location of that flag.

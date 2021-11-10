# ComponentTamperDetection
A Unity3d package for component-changes detection

## Wishful thinking

Do you ever wish you could see what's changed in a MonoBehaviour that should not change, ever? Or did it ever happen that your game stopped working because a MonoBehaviour-reference is suddenly 'None'? Or some other value, that you thought you could count on, is different? Because that never happens, right?

If so, then ComponentTamperDetection is for you.

It will make it possible to 'lock' a MonoBehaviour's values in time, and make you aware of it any of the values change. 

Simply add the component, ...

![AddComponent](Documentation/addComponent.png)

... drag and drop a MonoBehaviour into the 'Script Reference' field, ...

![DragComponent](Documentation/dragComponent.png)

... and when you're done developing whatever you're working on, press that 'Lock'-button:

![Lock](Documentation/lockButton.png)

This is one for the single dev, or small team, who develops a game and tests it manually and maybe in lots of unit-testing too, and want to 'lock it down' somehow before distributing it; so we can know, visually and also in automated testing (more on that below) that those MonoBehaviours and their values that we've carefully set, that is at the heart of our gameplay, won't change without us being in the know about it. 

Let's face it, we've all developed a feature and it's in version control, and we're thinking we're safe and sound - but  _still_ some value gets changed that shouldn't have, and without some kind of test - visual or automatic - it can be a cruel time-waste, hunting for that change. This component offers both a visual and a automatic change-detection. More on both later, but first a disclaimer: this is not a 'lock down' of a MonoBehaviour in the literal sense. You will still be able to - as you should - change the values of a MonoBehaviour you've 'locked' - but you can be sure the component will let you know about it. 

It is, at its heart, a tool that's meant to make you sleep better at night. It will arm you with the knowledge that something you thought was set in stone isn't so anymore, and give you a chance to react before it goes out into production.


## What's in the box?

The component works by traversing the public fields and storing a [hash-code](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=net-5.0) of their value. Although not guarenteed to be unique, they'll do fine for this use-case.

The 'Display/Hide component status'-button offers a peak of the calculated hash-codes:

![componentStatus](Documentation/componentStatus.png)

### When are value-changes detected?

Right off the bat you won't see changed detected. 

#### Fixed change detection

TODO

#### Dynamic change detection

TODO

## Unit-testing

TODO

## A few hints

"addChangeDetectors" hint

TODO
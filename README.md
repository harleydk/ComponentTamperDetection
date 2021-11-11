# ComponentTamperDetection
A Unity3d package for component-changes detection

## Wishful thinking

Do you ever wish you could see what's changed in a MonoBehaviour that should not change, ever? Or did it ever happen that your game stopped working because a MonoBehaviour-reference is suddenly 'None' where it should reference another component? Or some other value, that you thought you could count on, is different? Because that never happens, right?

If so, then ComponentTamperDetection is for you.

It will make it possible to 'lock' a MonoBehaviour's values in time, and offer visual clue if any of the values change. 

Simply add the component, ...

![AddComponent](Documentation/addComponent.png)

... drag and drop a MonoBehaviour into the 'Script Reference' field, ...

![DragComponent](Documentation/dragComponent.png)

... and when you're done developing whatever you're working on, press that 'Lock'-button:

![Lock](Documentation/lockButton.png)

The value from the referenced MonoBehaviour are stored into the ComponentTamperDetection component, from where we compare them with later changes. In which case the big red 'Not locked' button displaces the green-is-good 'Locked' one. A decent visual clue - but we can also do automatic unit-testing based on this; more on this later.

This is one for the single dev, or small team, who develops a game and tests it manually and maybe in lots of unit-testing too, and want to 'lock it down' somehow before distributing it; so we can know, visually and also in automated testing (more on that below) that those MonoBehaviours and their values that we've carefully set, that is at the heart of our gameplay, won't change without us being in the know about it. 

Let's face it, we've all developed a feature and it's in version control, and we're thinking we're safe and sound - but  _still_ some value gets changed that shouldn't have, and without some kind of test - visual or automatic - it can be a cruel time-waste, hunting for that change. This component offers both a visual and automatic change-detection. A disclaimer: this is not a 'lock down' of a MonoBehaviour in the literal sense. You will still be able to - as you should - change the values of a MonoBehaviour you've 'locked' - but you can be sure the component will let you know about it. 

_It is a tool that's meant to make you sleep better at night. It will arm you with the knowledge that something you thought was set in stone isn't so anymore, and give you a chance to react before it slips out into production._


## What's in the box?

The component works by traversing the public fields and storing a [hash-code](https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=net-5.0) of their value. Although not guarenteed to be unique, they'll do fine for this use-case.

The 'Display/Hide component status'-button offers a peak of the calculated hash-codes:

![componentStatus](Documentation/componentStatus.png)

Currently, the following types are covered: _string, float, int, bool, Color, Rect, Vector3, Vector2, UnityEvents, as well as references to either a Gameobject, MonoBehaviour, or Component. Also, collections with any of the above types.


### When are value-changes detected?

There are two types of value-detections. 

#### Fixed change detection

This is the default value detection method. The referenced MonoBehaviour has no out-of-the-box way of telling the ComponentTamperDetection that it's values has changed, so any changes are calculated when the scene is loaded. So, if you have locked a component and then proceed to change a value of the associated MonoBehaviour, the green 'Locked' label will change to a red 'Not locked' label only when the scene is reloaded. Although not ideal, at least you will be in the know.

#### Dynamic change detection

The ideal detection method is letting the referenced MonoBehaviour tell the ComponentTamperDetection that some values have changed. If you implement the IComponentTamperDetection-interface and add the appropriate code to call the ComponentTamperDetection when changes are made ...

```
using harleydk.ComponentTamperDetection;
using System;
using UnityEngine;

namespace Assets
{
    public class GenericTest : MonoBehaviour, IComponentTamperDetection
    {
        public event Action OnEditorValuesChanged;

        public void OnValidate()
        {
            if (Application.isEditor && OnEditorValuesChanged != null)
                OnEditorValuesChanged.Invoke();
        }

        public float TestFloat;
        public int TestInt;
    }
}
```

... the ComponentTamperDetection will react dynamically to any editor-changes made:

![DynamicChanges](Documentation/DynamicChanges.gif)

This is the preferred option, as it will immediately give you a visual clue that something is amiss.

## Unit-testing

Visual observation is one thing - but when it gets time to distribute your game, you would ideally have a number of automated tests you can execute, that will give you confidence you're shipping a stable product.

The ComponentTamperDetection works well in this regard. Simply add a [unit-test](https://docs.unity3d.com/Manual/testing-editortestsrunner.html) to iterate through all the ComponentTamperDetections and sound the alarm if a supposedly locked-down component has changed, unbeknownst to you:

```
[Test]
public void ComponentsWithTamperDetectionHaveNotChanged()
{
    // arrange
    var componentTamperDetectors = GameObject.FindObjectsOfType<ComponentTamperDetection>()
        .Where(c => c.ScriptReference != null);

    // act
    List<string> changedComponents = new List<string>();
    foreach (var componentTamperDetector in componentTamperDetectors)
    {
        bool hasComponentChanged = componentTamperDetector.HasComponentChanged();
        if (hasComponentChanged)
            changedComponents.Add($"{componentTamperDetector.gameObject.name}|{componentTamperDetector.name}|{componentTamperDetector.ScriptReference.name}");
    }

    // assert
    bool hasAnyComponentsBeenTamperedwith = changedComponents.Any();
    Assert.IsFalse(hasAnyComponentsBeenTamperedwith, $"The following ComponentTamperDetectors reported changed values." +
        $" If they seem fine, lock them down, else restore them to their former glory. \n" +
        $"{string.Join(System.Environment.NewLine, changedComponents)}."); // no components should've been tampered with
}

```
## I get by with a little help from my friends

<script type="text/javascript" src="https://cdnjs.buymeacoffee.com/1.0.0/button.prod.min.js" data-name="bmc-button" data-slug="Ghi82pFzV" data-color="#FFDD00" data-emoji=""  data-font="Cookie" data-text="Buy me a coffee" data-outline-color="#000000" data-font-color="#000000" data-coffee-color="#ffffff" ></script>


## On GameObject comparison

TODO describe GuidCreator
TODO "addChangeDetectors" hint
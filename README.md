# Unity Object Pooler
<!-- TABLE OF CONTENTS -->
## Table of Contents

* [About the Project](#about-the-project)
* [Getting Started](#getting-started)
* [Usage](#usage)
* [License](#license)
* [Contact](#contact)


<!-- ABOUT THE PROJECT -->
## About The Project

This is a simple object pooling system implementation for Unity with a bit of automatic prefab processing. Having created similar systems in past projects, this is an attempt at an implementation that can be used more generally.


<!-- GETTING STARTED -->
## Getting Started

Import everything into your Unity Assets folder (make sure to keep it all in the same folder).

Drop some prefabs you want to pool into the `Prefabs` folder.

Add `ObjectPooler.prefab` to your scene.

You're ready to start pooling!


<!-- USAGE EXAMPLES -->
## Usage
Objects can be retrieved and returned from anywhere with a couple simple methods.
*Note that the `ObjectPooler.cs` script initializes itself in its `Awake()` method.*

To retrieve an object from the pool use the following:
```
ObjectPooler.GetObjectFromPool("Name Of Prefab");
```
For convenience, the names of prefabs placed in the `Prefabs` folder are assigned to static strings in the `PrefabKeys.cs` script.

This allows object retrieval as in the following:
```
ObjectPooler.GetObjectFromPool(PrefabKeys.NameOfPrefab);
```
If no object in the chosen pool is available, a new one will be created.

When you want to return an object to the pool call the `ReturnObjectToPool` method:
```
ObjectPooler.ReturnObjectToPool(ObjectToReturn)
//The pool to which to retrun the object is derived from its name

//You can optionally specify the pool
ObjectPooler.ReturnObjectToPool(ObjectToReturn, "Name Of Prefab")

```
You can set the Object Pools to prepopulate by adjusting `Preloaded Objects` on the `ObjectPooler` object in your scene
### Limitations
Currently, only objects placed in the `Prefabs` folder are poolable.

In order to make the automatic writing of prefab names to static strings in the `PrefabKeys.cs` script work, spaces in prefab names are ignored. This means having two prefabs with the names "NameOfPrefab" and "Name Of Prefab", will cause an error.


<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE` for more information.

<!-- CONTACT -->
## Contact

Keith Glazewski - [@KeithComet](https://twitter.com/KeithComet)
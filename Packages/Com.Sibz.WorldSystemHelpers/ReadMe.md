
# World System Helpers

Contains the following extension methods to assist with 
creating systems in a world:

- `void WorldExtensions.ThrowIfNotComponentSystem(Type systemType)`
- `void world.CreateInGroup(Type systemType, ComponentSystemGroup group)`
- `bool world.TryCreateInGroup<TUpdateGroup>(Type systemType)`
- `bool world.TryGetExistingSystem<T>(Type systemType, out T system)`
- `bool world.TryCreateInGroupUsingUpdateInGroupAttribute(Type systemType)`
- `void world.ImportSystemsWithAttribute<TAttribute, TDefaultGroup>(this World world)`
- `void ImportSystemsFromList<TDefaultGroup>(this World world, IEnumerable<Type> list)`

Usage examples can be seen in Tests. Per method documentation in source.

##Changes 30/03/2020
Added ImportSystemsFromList 
 
##Changes 29/03/2020
Added some non generic variants. 
  
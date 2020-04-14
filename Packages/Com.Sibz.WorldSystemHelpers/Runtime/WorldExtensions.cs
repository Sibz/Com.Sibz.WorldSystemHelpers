using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using UnityEngine;

namespace Sibz.WorldSystemHelpers
{
    public static class WorldExtensions
    {
        /// <summary>
        /// Imports all systems into world that have a specific attribute.
        /// Uses the update in group attribute if present, otherwise defaults to `TDefaultGroup`
        /// </summary>
        /// <param name="world"></param>
        /// <typeparam name="TAttribute">Attribute type, MUST be in the assembly with the systems required</typeparam>
        /// <typeparam name="TDefaultGroup"></typeparam>
        public static void ImportSystemsWithAttribute<TAttribute, TDefaultGroup>(this World world)
            where TAttribute : Attribute
            where TDefaultGroup : ComponentSystemGroup=>
            world.ImportSystemsWithAttribute(typeof(TAttribute), typeof(TDefaultGroup));

        public static void ImportSystemsWithAttribute<TDefaultGroup>(this World world, Type attr)
            where TDefaultGroup : ComponentSystemGroup =>
            ImportSystemsWithAttribute(world, attr, typeof(TDefaultGroup));

        public static void ImportSystemsWithAttribute(this World world, Type attr, Type defaultGroup)
        {
            var systems = Assembly
                .GetAssembly(attr)
                .GetTypes()
                .Where(x => !(x.GetCustomAttribute(attr) is null));
            ImportSystemsFromList(world, systems, defaultGroup);
        }

        /// <summary>
        /// Imports using a list of ComponentSystem types
        /// </summary>
        /// <param name="world"></param>
        /// <param name="list"></param>
        /// <param name="defaultGroup"></param>
        /// <param name="remap">Remap target update in group attribute</param>
        /// <typeparam name="TDefaultGroup"></typeparam>
        public static void ImportSystemsFromList<TDefaultGroup>(this World world, IEnumerable<Type> list, Dictionary<Type, Type> remap = null)
            where TDefaultGroup : ComponentSystemGroup => ImportSystemsFromList(world, list, typeof(TDefaultGroup), remap);
        public static void ImportSystemsFromList(this World world, IEnumerable<Type> systems, Type defaultGroup, Dictionary<Type, Type> remap = null)
        {
            var array = systems.ToArray();
            foreach (Type systemType in array)
            {
                if (world.TryCreateInGroupUsingUpdateInGroupAttribute(systemType, remap))
                {
                    continue;
                }

                world.TryCreateInGroup(systemType, defaultGroup);
            }
        }


        /// <summary>
        /// Tries to insert a group of type `systemType` into the group specified in its
        /// `UpdateInGroup` attribute - if it has one.
        /// Logs a warning if the group specified in the `UpdateInGroup` attribute
        /// does not exist.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="systemType">Type of the system to create and insert</param>
        /// <returns>true if inserted, false if not</returns>
        public static bool TryCreateInGroupUsingUpdateInGroupAttribute(this World world, Type systemType, Dictionary<Type, Type> remap = null)
        {
            ThrowIfNotComponentSystem(systemType);

            if (!(systemType.GetCustomAttribute<UpdateInGroupAttribute>() is UpdateInGroupAttribute att))
            {
                // Doesn't have the UpdateInGroupAttribute
                return false;
            }

            Type groupType = att.GroupType;
            if (!(remap is null) && remap.ContainsKey(groupType))
            {
                groupType = remap[groupType];
            }

            if (world.TryGetExistingSystem(groupType, out ComponentSystemGroup cs))
            {
                world.CreateInGroup(systemType, cs);
                return true;
            }

            string groupTypeRemapped = groupType != att.GroupType ? $"(Remapped from {att.GroupType.Name}) " : "";
            Debug.LogWarning(
                $"Unable to update system {systemType.Name} in group {groupType.Name} {groupTypeRemapped}as it does not exist.");

            return false;
        }

        /// <summary>
        /// Gets a system if it exists. Generic designed so can use a base class as `T` parameter.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="systemType">Type of system</param>
        /// <param name="system">The existing system</param>
        /// <typeparam name="T">Type (or base type) of system</typeparam>
        /// <returns>True if retrieved, false if not</returns>
        public static bool TryGetExistingSystem<T>(this World world, Type systemType, out T system)
            where T : ComponentSystemBase
        {
            ThrowIfNotComponentSystem(systemType);

            return ((system = world.GetExistingSystem(systemType) as T) is ComponentSystemBase);
        }

        /// <summary>
        /// Creates and inserts a system in a group of type `TUpdateGroup` if it exists.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="systemType">Type of system to create and insert</param>
        /// <typeparam name="TUpdateGroup">Update group to insert into</typeparam>
        /// <returns>True if created, false if not. Warns if group doesn't exist</returns>
        public static bool TryCreateInGroup<TUpdateGroup>(this World world, Type systemType)
            where TUpdateGroup : ComponentSystemGroup
        {
            return world.TryCreateInGroup(systemType, typeof(TUpdateGroup));
        }
        public static bool TryCreateInGroup(this World world, Type systemType, Type updateGroup)
        {
            ThrowIfNotComponentSystem(systemType);

            if (world.GetExistingSystem(updateGroup) is ComponentSystemGroup g)
            {
                world.CreateInGroup(systemType, g);
                return true;
            }

            Debug.LogWarning(
                $"Unable to update system {systemType.Name} in group {updateGroup.Name} as it does not exist.");
            return false;
        }

        /// <summary>
        /// Creates as system and adds it to provided component system group
        /// </summary>
        /// <param name="world"></param>
        /// <param name="systemType"></param>
        /// <param name="componentSystemGroup"></param>
        /// <exception cref="ArgumentNullException">Thrown if componentSystemGroup or systemType is null</exception>
        /// <exception cref="ArgumentException">Thrown if systemType is not a ComponentSystemBase</exception>
        public static void CreateInGroup(this World world, Type systemType, ComponentSystemGroup componentSystemGroup)
        {
            ThrowIfNotComponentSystem(systemType);

            if (componentSystemGroup is null)
            {
                throw new ArgumentNullException(nameof(componentSystemGroup));
            }

            componentSystemGroup.AddSystemToUpdateList(world.CreateSystem(systemType));
            componentSystemGroup.SortSystemUpdateList();
        }

        /// <summary>
        /// Checks a system is not null and is a subclass of `ComponentSystemBase`
        /// </summary>
        /// <param name="systemType"></param>
        /// <exception cref="ArgumentNullException">Thrown when systemType is null</exception>
        /// <exception cref="ArgumentException">Thrown when systemType is not `ComponentSystemBase`</exception>
        public static void ThrowIfNotComponentSystem(Type systemType)
        {
            if (systemType is null)
            {
                throw new ArgumentNullException(nameof(systemType));
            }

            if (!systemType.IsSubclassOf(typeof(ComponentSystemBase)))
            {
                throw new ArgumentException(
                    $"Param '{nameof(systemType)}' must be a subclass of '{nameof(ComponentSystemBase)}'",
                    nameof(systemType));
            }
        }
    }
}
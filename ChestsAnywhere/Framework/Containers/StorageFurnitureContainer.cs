using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Pathoschild.Stardew.ChestsAnywhere.Framework.Containers
{
    /// <summary>A storage container for a <see cref="StorageFurniture"/> instance (e.g. a dresser).</summary>
    internal class StorageFurnitureContainer : IContainer
    {
        /*********
        ** Fields
        *********/
        /// <summary>The in-game storage furniture.</summary>
        internal readonly StorageFurniture Furniture;

        /// <summary>The categories accepted by a dresser.</summary>
        private static HashSet<int> DresserCategories = null!; // set when the class is first constructed


        /*********
        ** Accessors
        *********/
        /// <inheritdoc />
        public IList<Item?> Inventory => this.Furniture.heldItems;

        /// <inheritdoc />
        public ContainerData Data { get; }

        /// <inheritdoc />
        public bool CanConfigureAutomate { get; } = false; // Automate doesn't support storage containers


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="furniture">The in-game storage furniture.</param>
        [SuppressMessage("ReSharper", "ConstantNullCoalescingCondition", Justification = $"{nameof(StorageFurnitureContainer.DresserCategories)} is only non-null after the first instance is constructed.")]
        public StorageFurnitureContainer(StorageFurniture furniture)
        {
            this.Furniture = furniture;
            this.Data = new ContainerData(furniture.modData);

            StorageFurnitureContainer.DresserCategories ??= new HashSet<int>(new ShopMenu(new List<ISalable>(), context: "Dresser").categoriesToSellHere);
        }

        /// <inheritdoc />
        public bool CanAcceptItem(Item item)
        {
            return StorageFurnitureContainer.DresserCategories.Contains(item.Category);
        }

        /// <inheritdoc />
        public bool IsSameAs(IContainer? container)
        {
            return
                container is not null
                && this.IsSameAs(container.Inventory);
        }

        /// <inheritdoc />
        public bool IsSameAs(IList<Item?>? inventory)
        {
            return
                inventory is not null
                && object.ReferenceEquals(this.Inventory, inventory);
        }

        /// <inheritdoc />
        public IClickableMenu OpenMenu()
        {
            Dictionary<ISalable, int[]> itemPriceAndStock = this.Furniture.heldItems
                .OfType<ISalable>() // cast as ISalable, and also ignore null in rare cases
                .ToDictionary(item => item, _ => new[] { 0, 1 });

            ShopMenu menu = new ShopMenu(itemPriceAndStock, 0, null, this.Furniture.onDresserItemWithdrawn, this.Furniture.onDresserItemDeposited, this.Furniture.GetShopMenuContext())
            {
                source = this.Furniture,
                behaviorBeforeCleanup = _ => this.Furniture.mutex.ReleaseLock()
            };

            Game1.activeClickableMenu = menu;
            return menu;
        }

        /// <inheritdoc />
        public void SaveData()
        {
            this.Data.ToModData(this.Furniture.modData);
        }
    }
}

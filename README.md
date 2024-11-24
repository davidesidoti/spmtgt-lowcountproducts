# LowCountProducts Mod for Supermarket Together

![Steam](https://img.shields.io/badge/steam-%23000000.svg?style=for-the-badge&logo=steam&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white)
![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white)
![Rider](https://img.shields.io/badge/Rider-000000.svg?style=for-the-badge&logo=Rider&logoColor=white&color=black&labelColor=crimson)

**Version:** 0.1

A BepInEx mod for the game **Supermarket Together** that automates inventory management by adding low stock products to your shopping list with a single keypress.

## Features

- **Automated Inventory Management:** Scans all unlocked product categories to identify low stock items.
- **Customizable Threshold:** Detects products with shelf quantities less than or equal to 10 and zero quantities in storage and boxes.
- **One-Key Operation:** Adds all identified low stock products to your shopping list instantly.
- **Duplicate Prevention:** Ensures the same product isn't added multiple times to your shopping list.
- **Toggle Functionality:** Easily enable or disable the mod's features using configurable keybindings.
- **In-Game Notifications:** Provides notifications to confirm actions and updates.

## Installation

1. **Install BepInEx:**

   - Download the BepInEx pack compatible with your game version from the [BepInEx Releases](https://github.com/BepInEx/BepInEx/releases) page.
   - Extract the contents of the BepInEx zip file into your game directory. The directory should now contain a `BepInEx` folder alongside the game executable.

2. **Download the Mod:**

   - [Download the latest release](https://github.com/davidesidoti/spmtgt-lowcountproducts/releases) of the LowCountProducts mod.

3. **Install the Mod:**

   - Place the `LowCountProducts.dll` file into the `BepInEx/plugins` folder within your game directory.

## Usage

- **Toggle Low Count Products Monitoring:**

  - **Default Keybind:** `B`
  - Press `B` to enable or disable the monitoring of low count products.

- **Add Low Count Products to Shopping List:**

  - **Default Keybind:** `Left Ctrl + B`
  - Press `Left Ctrl + B` to add all low count products from all unlocked categories to your shopping list.

## Configuration

- After the first launch, a configuration file named `SupermarketTogether.plugins.lowcountproducts.cfg` will be created in the `BepInEx/config` directory.
- You can edit this file to customize the keybindings:

  ```ini
  [General]

  ## Toggle Low Count Products Monitoring
  # Setting type: KeyboardShortcut
  # Default value: B
  LowCountProducts Toggle = B

  ## Add Low Count Products to Cart
  # Setting type: KeyboardShortcut
  # Default value: LeftControl + B
  AddLowCountProducts Key = LeftControl + B

  # Setting type: Int32
  # Default value: 10
  LowCountProducts Threshold = 10
  ```

## How It Works

- **Scanning Products:**
  - The mod accesses all unlocked products in the game.
  - It checks the quantities of each product on shelves, in storage, and in boxes.
  - A product is, by default, considered low in stock if:
    - Shelf quantity is ≤ **10**
    - Storage quantity is **0**
    - Boxes quantity is **0**
      
- **Adding to Shopping List:**
  - When you press the keybind to add low count products, the mod:
    - Adds all identified low stock products to your shopping list.
    - Checks to ensure no duplicates are added if the keybind is pressed multiple times.
   
- **Notifications:**
  - The mod provides in-game notifications to inform you about:
    - The toggling of low count product monitoring.
    - The addition of products to your shopping list.
   
## Compatibility

- **Game Version:** Tested with the latest version (December 2024) of Supermarket Together.
- **BepInEx Version:** Requires BepInEx 5.4 or higher.
- **Harmony Version:** Uses Harmony for method patching.

## Contributing

- **Bug Reports:** If you encounter any issues or bugs, please open an issue on GitHub.
- **Feature Requests:** Have an idea to improve the mod? Feel free to submit a feature request.

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](https://github.com/davidesidoti/spmtgt-lowcountproducts/blob/main/LICENSE) file for details.

## Acknowledgments

**BepInEx** - The modding framework used to create this mod.
**Harmony** - For providing method patching capabilities.
**Community** - Thanks to all the testers and contributors who helped improve this mod.

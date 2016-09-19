using System.Collections.Generic;
using Windows.Devices.Adc.Provider;

namespace Microsoft.IoT.AdcMcp3208 {
    public sealed class AdcMcp3208Provider : IAdcProvider {
        static IAdcProvider providerSingleton = null;

        static public IAdcProvider GetAdcProvider() {
            if (providerSingleton == null) {
                providerSingleton = new AdcMcp3208Provider();
                }
            return providerSingleton;

            }

        public IReadOnlyList<IAdcControllerProvider> GetControllers() {
            AdcMcp3208ControllerProvider provider = new AdcMcp3208ControllerProvider(AdcMcp3208ControllerProvider.DefaultChipSelectLine);

            List<IAdcControllerProvider> list = new List<IAdcControllerProvider>();
            list.Add(provider);

            return list;
            }
        }
    }

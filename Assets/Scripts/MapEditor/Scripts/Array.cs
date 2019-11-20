﻿using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using RustMapEditor.Variables;
using System.Threading;

namespace RustMapEditor.Maths
{
    public static class Array
    {
        /// <summary>Sets all the elements in the selected area of the array to the specified value.</summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] SetValues(float[,] array, float value, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            return array;
        }
        public static float[,,] SetValues(float[,,] array, int channel, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            int channelLength = array.GetLength(2);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        for (int k = 0; k < channelLength; k++)
                        {
                            array[i, j, k] = 0;
                        }
                        array[i, j, channel] = 1;
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        for (int k = 0; k < channelLength; k++)
                        {
                            array[i, j, k] = 0;
                        }
                        array[i, j, channel] = 1;
                    }
                });
            }
            return array;
        }
        public static bool[,] SetValues(bool[,] array, bool value, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        array[i, j] = value;
                    }
                });
            }
            return array;
        }
        /// <summary>Sets all the elements with values within the range limits on the channel selected.</summary>
        /// <param name="range">Array of values to check against the range limits.</param>
        /// <param name="channel">The channel to set the values to.</param>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,,] SetRange(float[,,] array, float[,] range, int channel, float rangeLow, float rangeHigh, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            int channelCount = array.GetLength(2);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        if (range[i, j] > rangeLow && range[i, j] < rangeHigh)
                        {
                            for (int k = 0; k < channelCount; k++)
                            {
                                array[i, j, k] = 0;
                            }
                            array[i, j, channel] = 1;
                        }
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        if (range[i, j] > rangeLow && range[i, j] < rangeHigh)
                        {
                            for (int k = 0; k < channelCount; k++)
                            {
                                array[i, j, k] = 0;
                            }
                            array[i, j, channel] = 1;
                        }
                    }
                });
            }
            return array;
        }
        public static bool[,] SetRange(bool[,] array, float[,] range, bool value, float rangeLow, float rangeHigh, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        if (range[i, j] > rangeLow && range[i, j] < rangeHigh)
                        {
                            array[i, j] = value;
                        }
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        if (range[i, j] > rangeLow && range[i, j] < rangeHigh)
                        {
                            array[i, j] = value;
                        }
                    }
                });
            }
            return array;
        }
        public static float[,,] SetRiver(float[,,] array, float[,] landHeights, float[,] waterHeights, bool aboveTerrain, int channel, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            int channelLength = array.GetLength(2);
            if (dmns != null)
            {
                if (aboveTerrain)
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            if (waterHeights[i, j] > 500 && waterHeights[i, j] > landHeights[i, j])
                            {
                                for (int k = 0; k < channelLength; k++)
                                {
                                    array[i, j, k] = 0;
                                }
                                array[i, j, channel] = 1;
                            }
                        }
                    });
                }
                else
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            if (waterHeights[i, j] > 500)
                            {
                                for (int k = 0; k < channelLength; k++)
                                {
                                    array[i, j, k] = 0;
                                }
                                array[i, j, channel] = 1;
                            }
                        }
                    });
                }
            }
            else
            {
                if (aboveTerrain)
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            if (waterHeights[i, j] > 500 && waterHeights[i, j] > landHeights[i, j])
                            {
                                for (int k = 0; k < channelLength; k++)
                                {
                                    array[i, j, k] = 0;
                                }
                                array[i, j, channel] = 1;
                            }
                        }
                    });
                }
                else
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            if (waterHeights[i, j] > 500)
                            {
                                for (int k = 0; k < channelLength; k++)
                                {
                                    array[i, j, k] = 0;
                                }
                                array[i, j, channel] = 1;
                            }
                        }
                    });
                }
            }
            return array;
        }
        public static bool[,] SetRiver(bool[,] array, float[,] landHeights, float[,] waterHeights, bool aboveTerrain, bool value, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                if (aboveTerrain)
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            if (waterHeights[i, j] > 500 && waterHeights[i, j] > landHeights[i, j])
                            {
                                array[i, j] = value;
                            }
                        }
                    });
                }
                else
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            if (waterHeights[i, j] > 500)
                            {
                                array[i, j] = value;
                            }
                        }
                    });
                }
            }
            else
            {
                if (aboveTerrain)
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            if (waterHeights[i, j] > 500 && waterHeights[i, j] > landHeights[i, j])
                            {
                                array[i, j] = value;
                            }
                        }
                    });
                }
                else
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            if (waterHeights[i, j] > 500)
                            {
                                array[i, j] = value;
                            }
                        }
                    });
                }
            }
            return array;
        }
        /// <summary>Clamps all the values to within the set range.</summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] ClampValues(float[,] array, float minValue, float maxValue, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = Mathf.Clamp(array[i, j], minValue, maxValue);
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        array[i, j] = Mathf.Clamp(array[i, j], minValue, maxValue);
                    }
                });
            }
            return array;
        }
        /// <summary>Rotates the array CW or CCW.</summary>
        /// <param name="CW">CW = 90°, CCW = 270°</param>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Rotate(float[,] array, bool CW, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            float[,] newArray = new float[array.GetLength(0), array.GetLength(1)];
            if (dmns != null)
            {
                if (CW)
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            newArray[i, j] = array[j, arrayLength - i - 1];
                        }
                    });
                }
                else
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            newArray[i, j] = array[arrayLength - j - 1, i];
                        }
                    });
                }
            }
            else
            {
                if (CW)
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            newArray[i, j] = array[j, arrayLength - i - 1];
                        }
                    });
                }
                else
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            newArray[i, j] = array[arrayLength - j - 1, i];
                        }
                    });
                }
            }
            return newArray;
        }
        public static float[,,] Rotate(float[,,] array, bool CW, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            int channelLength = array.GetLength(2);
            float[,,] newArray = new float[array.GetLength(0), array.GetLength(1), array.GetLength(2)];
            if (dmns != null)
            {
                if (CW)
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            for (int k = 0; k < channelLength; k++)
                            {
                                newArray[i, j, k] = array[j, arrayLength - i - 1, k];
                            }
                        }
                    });
                }
                else
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            for (int k = 0; k < channelLength; k++)
                            {
                                newArray[i, j, k] = array[arrayLength - j - 1, i, k];
                            }
                        }
                    });
                }
            }
            else
            {
                if (CW)
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            for (int k = 0; k < channelLength; k++)
                            {
                                newArray[i, j, k] = array[j, array.GetLength(1) - i - 1, k];
                            }
                        }
                    });
                }
                else
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            for (int k = 0; k < channelLength; k++)
                            {
                                newArray[i, j, k] = array[arrayLength - j - 1, i, k];
                            }
                        }
                    });
                }
            }
            return newArray;
        }
        public static bool[,] Rotate(bool[,] array, bool CW, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            bool[,] newArray = new bool[array.GetLength(0), array.GetLength(1)];
            if (dmns != null)
            {
                if (CW)
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            newArray[i, j] = array[j, arrayLength - i - 1];
                        }
                    });
                }
                else
                {
                    Parallel.For(dmns.x0, dmns.x1, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            newArray[i, j] = array[arrayLength - j - 1, i];
                        }
                    });
                }
            }
            else
            {
                if (CW)
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            newArray[i, j] = array[j, arrayLength - i - 1];
                        }
                    });
                }
                else
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            newArray[i, j] = array[arrayLength - j - 1, i];
                        }
                    });
                }
            }
            return newArray;
        }
        /// <summary>Flips the values of the array.</summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Invert(float[,] array, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = 1 - array[i, j];
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        array[i, j] = 1 - array[i, j];
                    }
                });
            }
            return array;
        }
        public static float[,,] Invert(float[,,] array, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            int channelLength = array.GetLength(2);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        for (int k = 0; k < channelLength; k++)
                        {
                            array[i, j, k] = 1 - array[i, j, k];
                        }
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        for (int k = 0; k < channelLength; k++)
                        {
                            array[i, j, k] = 1 - array[i, j, k];
                        }
                    }
                });
            }
            return array;
        }
        public static bool[,] Invert(bool[,] array, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = !array[i, j];
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        array[i, j] = !array[i, j];
                    }
                });
            }
            return array;
        }
        /// <summary>Normalises the values of the array between 2 floats.</summary>
        /// <param name="normaliseLow">Min value of the array.</param>
        /// <param name="normaliseHigh">Max value of the array.</param>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        public static float[,] Normalise(float[,] array, float normaliseLow, float normaliseHigh, Dimensions dmns = null)
        {
            float highestPoint = 0f, lowestPoint = 1f, heightRange = 0f, normalisedHeightRange = 0f;
            int arrayLength = array.GetLength(0);
            if (dmns != null)
            {
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        if (array[i, j] < lowestPoint)
                        {
                            lowestPoint = array[i, j];
                        }
                        else if (array[i, j] > highestPoint)
                        {
                            highestPoint = array[i, j];
                        }
                    }
                });
                heightRange = highestPoint - lowestPoint;
                normalisedHeightRange = normaliseHigh - normaliseLow;
                Parallel.For(dmns.x0, dmns.x1, i =>
                {
                    for (int j = dmns.z0; j < dmns.z1; j++)
                    {
                        array[i, j] = normaliseLow + ((array[i, j] - lowestPoint) / heightRange) * normalisedHeightRange;
                    }
                });
            }
            else
            {
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        if (array[i, j] < lowestPoint)
                        {
                            lowestPoint = array[i, j];
                        }
                        else if (array[i, j] > highestPoint)
                        {
                            highestPoint = array[i, j];
                        }
                    }
                });
                heightRange = highestPoint - lowestPoint;
                normalisedHeightRange = normaliseHigh - normaliseLow;
                Parallel.For(0, arrayLength, i =>
                {
                    for (int j = 0; j < arrayLength; j++)
                    {
                        array[i, j] = normaliseLow + ((array[i, j] - lowestPoint) / heightRange) * normalisedHeightRange;
                    }
                });
            }
            return array;
        }
        /// <summary>Offsets the values of the array by the specified value.</summary>
        /// <param name="dmns">The area of the array to perform the operations.</param>
        /// <param name="clampOffset">Prevent array values from overflowing.</param>
        public static float[,] Offset(float[,] array, float offset, bool clampOffset, Dimensions dmns = null)
        {
            int arrayLength = array.GetLength(0);
            float[,] tempArray = array;
            CancellationTokenSource source = new CancellationTokenSource();
            ParallelOptions options = new ParallelOptions() { CancellationToken = source.Token};
            try
            {
                if (dmns != null)
                {
                    Parallel.For(dmns.x0, dmns.x1, options, i =>
                    {
                        for (int j = dmns.z0; j < dmns.z1; j++)
                        {
                            if (clampOffset == true)
                            {
                                if ((array[i, j] + offset > 1f || array[i, j] + offset < 0f))
                                {
                                    source.Cancel();
                                }
                                else
                                {
                                    tempArray[i, j] += offset;
                                }
                            }
                            else
                            {
                                tempArray[i, j] += offset;
                            }
                        }
                    });
                }
                else
                {
                    Parallel.For(0, arrayLength, i =>
                    {
                        for (int j = 0; j < arrayLength; j++)
                        {
                            if (clampOffset == true)
                            {
                                if ((array[i, j] + offset > 1f || array[i, j] + offset < 0f))
                                {
                                    source.Cancel();
                                }
                                else
                                {
                                    tempArray[i, j] += offset;
                                }
                            }
                            else
                            {
                                tempArray[i, j] += offset;
                            }
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            {
                return array;
            }
            return tempArray;
        }
        public static float[,] ShortMapToFloatArray(TerrainMap<short> terrainMap)
        {
            float[,] array = new float[terrainMap.res, terrainMap.res];
            int arrayLength = array.GetLength(0);
            Parallel.For(0, arrayLength, i =>
            {
                for (int j = 0; j < arrayLength; j++)
                {
                    array[i, j] = BitUtility.Short2Float(terrainMap[i, j]);
                }
            });
            return array;
        }
        public static byte[] FloatArrayToByteArray(float[,] array)
        {
            short[] shortArray = new short[array.GetLength(0) * array.GetLength(1)];
            int arrayLength = array.GetLength(0);
            Parallel.For(0, arrayLength, i =>
            {
                for (int j = 0; j < arrayLength; j++)
                {
                    shortArray[(i * arrayLength) + j] = BitUtility.Float2Short(array[i, j]);
                }
            });

            byte[] byteArray = new byte[shortArray.Length * 2];

            Buffer.BlockCopy(shortArray, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }
        public static float[,,] SingleToMulti(float[] array, int texturesAmount)
        {
            int length = (int)Math.Sqrt(array.Length / texturesAmount);
            float[,,] multiArray = new float[length, length, texturesAmount];
            Parallel.For(0, length, i =>
            {
                for (int j = 0; j < length; j++)
                {
                    for (int k = 0; k < texturesAmount; k++)
                    {
                        multiArray[i, j, k] = array[i * length * texturesAmount + (j * texturesAmount + k)];
                    }
                }
            });
            return multiArray;
        }
        public static float[,,] NormaliseMulti(float[,,] array, int texturesAmount)
        {
            int length = (int)Math.Sqrt(array.Length / texturesAmount);
            int arrayLength = array.GetLength(0);
            int channelLength = array.GetLength(2);
            Parallel.For(0, array.GetLength(0), i =>
            {
                float[] splatWeights = new float[channelLength];
                for (int j = 0; j < arrayLength; j++)
                {
                    for (int k = 0; k < channelLength; k++)
                    {
                        splatWeights[k] = array[i, j, k];
                    }
                    float normalisedWeights = splatWeights.Sum();
                    for (int k = 0; k < channelLength; k++)
                    {
                        splatWeights[k] /= normalisedWeights;
                        array[i, j, k] = splatWeights[k];
                    }
                }
            });
            return array;
        }
        public static float[,,] BoolToMulti(bool[,] array)
        {
            float[,,] multiArray = new float[array.GetLength(0), array.GetLength(1), 2];
            int arrayLength = array.GetLength(0);
            Parallel.For(0, arrayLength, i =>
            {
                for (int j = 0; j < arrayLength; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        multiArray[i, j, k] = (array[i, j]) ? 1f : 0f;
                    }
                }
            });
            return multiArray;
        }
        public static bool[,] MultiToBool(float[,,] array)
        {
            bool[,] boolArray = new bool[array.GetLength(0), array.GetLength(1)];
            int arrayLength = array.GetLength(0);
            Parallel.For(0, arrayLength, i =>
            {
                for (int j = 0; j < arrayLength; j++)
                {
                    boolArray[i, j] = (array[i, j, 0] > 0.5f) ? true : false; 
                }
            });
            return boolArray;
        }
    }
}
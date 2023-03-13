using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace var_browser
{
	public class CustomImageLoaderThreaded : MonoBehaviour
	{
		public delegate void ImageLoaderCallback(QueuedImage qi);

		public class QueuedImage
		{
			public bool isThumbnail;

			public string imgPath;

			public bool skipCache;

			public bool forceReload;

			public bool createMipMaps;

			public bool compress = true;

			public bool linear;

			public bool processed;

			public bool preprocessed;

			public bool cancel;

			public bool finished;

			public bool isNormalMap;

			public bool createAlphaFromGrayscale;

			public bool createNormalFromBump;

			public float bumpStrength = 1f;

			public bool invert;

			public bool setSize;

			public bool fillBackground;

			public int width;

			public int height;

			public byte[] raw;

			public bool hadError;

			public string errorText;

			public TextureFormat textureFormat;

			public Texture2D tex;

			public RawImage rawImageToLoad;

			public bool useWebCache;

			public UnityWebRequest webRequest;

			public bool webRequestDone;

			public bool webRequestHadError;

			public byte[] webRequestData;

			public ImageLoaderCallback callback;

			public string cacheSignature
			{
				get
				{
					string text = imgPath;
					if (compress)
					{
						text += ":C";
					}
					if (linear)
					{
						text += ":L";
					}
					if (isNormalMap)
					{
						text += ":N";
					}
					if (createAlphaFromGrayscale)
					{
						text += ":A";
					}
					if (createNormalFromBump)
					{
						text = text + ":BN" + bumpStrength;
					}
					if (invert)
					{
						text += ":I";
					}
					return text;
				}
			}

			protected string diskCacheSignature
			{
				get
				{
					string text = ((!setSize) ? string.Empty : (width + "_" + height));
					if (compress)
					{
						text += "_C";
					}
					if (linear)
					{
						text += "_L";
					}
					if (isNormalMap)
					{
						text += "_N";
					}
					if (createAlphaFromGrayscale)
					{
						text += "_A";
					}
					if (createNormalFromBump)
					{
						text = text + "_BN" + bumpStrength;
					}
					if (invert)
					{
						text += "_I";
					}
					return text;
				}
			}

			protected string GetDiskCachePath()
			{
				string result = null;
				FileEntry fileEntry = FileManager.GetFileEntry(imgPath);
				string textureCacheDir =MVR.FileManagement.CacheManager.GetTextureCacheDir();
				if (fileEntry != null && textureCacheDir != null)
				{
					string text = fileEntry.Size.ToString();
					string text2 = fileEntry.LastWriteTime.ToFileTime().ToString();
					string text3 = textureCacheDir + "/";
					string fileName = Path.GetFileName(imgPath);
					fileName = fileName.Replace('.', '_');
					result = text3 + fileName + "_" + text + "_" + text2 + "_" + diskCacheSignature + ".vamcache";
				}
				return result;
			}

			protected string GetWebCachePath()
			{
				string result = null;
				string textureCacheDir = MVR.FileManagement.CacheManager.GetTextureCacheDir();
				if (textureCacheDir != null)
				{
					string text = imgPath.Replace("https://", string.Empty);
					text = text.Replace("http://", string.Empty);
					text = text.Replace("/", "__");
					text = text.Replace("?", "_");
					string text2 = textureCacheDir + "/";
					result = text2 + text + "_" + diskCacheSignature + ".vamcache";
				}
				return result;
			}

			public bool WebCachePathExists()
			{
				string webCachePath = GetWebCachePath();
				if (webCachePath != null && FileManager.FileExists(webCachePath))
				{
					return true;
				}
				return false;
			}

			public void CreateTexture()
			{
				if (tex == null)
				{
					try
					{
						tex = new Texture2D(width, height, textureFormat, createMipMaps, linear);
					}
					catch (Exception ex)
					{
						LogUtil.LogError(imgPath + " " + ex);
					}
					tex.name = cacheSignature;
				}
			}

			protected void ReadMetaJson(string jsonString)
			{
				JSONNode jSONNode = JSON.Parse(jsonString);
				JSONClass asObject = jSONNode.AsObject;
				if (asObject != null)
				{
					if (asObject["width"] != null)
					{
						width = asObject["width"].AsInt;
					}
					if (asObject["height"] != null)
					{
						height = asObject["height"].AsInt;
					}
					if (asObject["format"] != null)
					{
						textureFormat = (TextureFormat)Enum.Parse(typeof(TextureFormat), asObject["format"]);
					}
				}
			}

			protected void ProcessFromStream(Stream st)
			{
				Bitmap bitmap = new Bitmap(st);
				SolidBrush solidBrush = new SolidBrush(System.Drawing.Color.White);
				bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
				if (!setSize)
				{
					width = bitmap.Width;
					height = bitmap.Height;
					if (compress)
					{
						int num = width / 4;
						if (num == 0)
						{
							num = 1;
						}
						width = num * 4;
						int num2 = height / 4;
						if (num2 == 0)
						{
							num2 = 1;
						}
						height = num2 * 4;
					}
				}
				int num3 = 3;
				textureFormat = TextureFormat.RGB24;
				PixelFormat format = PixelFormat.Format24bppRgb;
				if (createAlphaFromGrayscale || isNormalMap || createNormalFromBump || bitmap.PixelFormat == PixelFormat.Format32bppArgb)
				{
					textureFormat = TextureFormat.RGBA32;
					format = PixelFormat.Format32bppArgb;
					num3 = 4;
				}
				Bitmap bitmap2 = new Bitmap(width, height, format);
				System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap2);
				Rectangle rect = new Rectangle(0, 0, width, height);
				if (setSize)
				{
					if (fillBackground)
					{
						graphics.FillRectangle(solidBrush, rect);
					}
					float num4 = Mathf.Min((float)width / (float)bitmap.Width, (float)height / (float)bitmap.Height);
					int num5 = (int)((float)bitmap.Width * num4);
					int num6 = (int)((float)bitmap.Height * num4);
					graphics.DrawImage(bitmap, (width - num5) / 2, (height - num6) / 2, num5, num6);
				}
				else
				{
					graphics.DrawImage(bitmap, 0, 0, width, height);
				}
				BitmapData bitmapData = bitmap2.LockBits(rect, ImageLockMode.ReadOnly, bitmap2.PixelFormat);
				int num7 = width * height;
				int num8 = num7 * num3;
				int num9 = Mathf.CeilToInt((float)num8 * 1.5f);
				raw = new byte[num9];
				Marshal.Copy(bitmapData.Scan0, raw, 0, num8);
				bitmap2.UnlockBits(bitmapData);
				bool flag = isNormalMap && num3 == 4;
				for (int i = 0; i < num8; i += num3)
				{
					byte b = raw[i];
					raw[i] = raw[i + 2];
					raw[i + 2] = b;
					if (flag)
					{
						raw[i + 3] = byte.MaxValue;
					}
				}
				if (invert)
				{
					for (int j = 0; j < num8; j++)
					{
						int num10 = 255 - raw[j];
						raw[j] = (byte)num10;
					}
				}
				if (createAlphaFromGrayscale)
				{
					for (int k = 0; k < num8; k += 4)
					{
						int num11 = raw[k];
						int num12 = raw[k + 1];
						int num13 = raw[k + 2];
						int num14 = (num11 + num12 + num13) / 3;
						raw[k + 3] = (byte)num14;
					}
				}
				if (createNormalFromBump)
				{
					byte[] array = new byte[num8 * 2];
					float[][] array2 = new float[height][];
					for (int l = 0; l < height; l++)
					{
						array2[l] = new float[width];
						for (int m = 0; m < width; m++)
						{
							int num15 = (l * width + m) * 4;
							int num16 = raw[num15];
							int num17 = raw[num15 + 1];
							int num18 = raw[num15 + 2];
							float num19 = (float)(num16 + num17 + num18) / 768f;
							array2[l][m] = num19;
						}
					}
					Vector3 vector = default(Vector3);
					for (int n = 0; n < height; n++)
					{
						for (int num20 = 0; num20 < width; num20++)
						{
							float num21 = 0.5f;
							float num22 = 0.5f;
							float num23 = 0.5f;
							float num24 = 0.5f;
							float num25 = 0.5f;
							float num26 = 0.5f;
							float num27 = 0.5f;
							float num28 = 0.5f;
							int num29 = num20 - 1;
							int num30 = num20 + 1;
							int num31 = n + 1;
							int num32 = n - 1;
							int num33 = num31;
							int num34 = num29;
							int num35 = num32;
							int num36 = num29;
							int num37 = num31;
							int num38 = num30;
							int num39 = num32;
							int num40 = num30;
							if (num33 >= 0 && num33 < height && num34 >= 0 && num34 < width)
							{
								num21 = array2[num33][num34];
							}
							if (num29 >= 0 && num29 < width)
							{
								num22 = array2[n][num29];
							}
							if (num35 >= 0 && num35 < height && num36 >= 0 && num36 < width)
							{
								num23 = array2[num35][num36];
							}
							if (num31 >= 0 && num31 < height)
							{
								num24 = array2[num31][num20];
							}
							if (num32 >= 0 && num32 < height)
							{
								num25 = array2[num32][num20];
							}
							if (num37 >= 0 && num37 < height && num38 >= 0 && num38 < width)
							{
								num26 = array2[num37][num38];
							}
							if (num30 >= 0 && num30 < width)
							{
								num27 = array2[n][num30];
							}
							if (num39 >= 0 && num39 < height && num40 >= 0 && num40 < width)
							{
								num28 = array2[num39][num40];
							}
							float num41 = num26 + 2f * num27 + num28 - num21 - 2f * num22 - num23;
							float num42 = num23 + 2f * num25 + num28 - num21 - 2f * num24 - num26;
							vector.x = num41 * bumpStrength;
							vector.y = num42 * bumpStrength;
							vector.z = 1f;
							vector.Normalize();
							vector.x = vector.x * 0.5f + 0.5f;
							vector.y = vector.y * 0.5f + 0.5f;
							vector.z = vector.z * 0.5f + 0.5f;
							int num43 = (int)(vector.x * 255f);
							int num44 = (int)(vector.y * 255f);
							int num45 = (int)(vector.z * 255f);
							int num46 = (n * width + num20) * 4;
							array[num46] = (byte)num45;
							array[num46 + 1] = (byte)num44;
							array[num46 + 2] = (byte)num43;
							array[num46 + 3] = byte.MaxValue;
						}
					}
					raw = array;
				}
				solidBrush.Dispose();
				graphics.Dispose();
				bitmap.Dispose();
				bitmap2.Dispose();
			}

			public void Process()
			{
				if (processed)
				{
					return;
				}
				if (imgPath != null && imgPath != "NULL")
				{
					if (useWebCache)
					{
						string webCachePath = GetWebCachePath();
						try
						{
							string text = webCachePath + "meta";
							if (FileManager.FileExists(text))
							{
								string jsonString = FileManager.ReadAllText(text);
								ReadMetaJson(jsonString);
								raw = FileManager.ReadAllBytes(webCachePath);
								preprocessed = true;
							}
							else
							{
								hadError = true;
								errorText = "Missing cache meta file " + text;
							}
						}
						catch (Exception ex)
						{
							LogUtil.LogError("Exception during cache file read " + ex);
							hadError = true;
							errorText = ex.ToString();
						}
					}
					else if (webRequest != null)
					{
						if (!webRequestDone)
						{
							return;
						}
						try
						{
							if (!webRequestHadError && webRequestData != null)
							{
								using (MemoryStream st = new MemoryStream(webRequestData))
								{
									ProcessFromStream(st);
								}
							}
						}
						catch (Exception ex2)
						{
							hadError = true;
							LogUtil.LogError("Exception " + ex2);
							errorText = ex2.ToString();
						}
					}
					else if (FileManager.FileExists(imgPath))
					{
						string diskCachePath = GetDiskCachePath();
						if (MVR.FileManagement.CacheManager.CachingEnabled && diskCachePath != null && FileManager.FileExists(diskCachePath))
						{
							try
							{
								string text2 = diskCachePath + "meta";
								if (FileManager.FileExists(text2))
								{
									string jsonString2 = FileManager.ReadAllText(text2);
									ReadMetaJson(jsonString2);
									raw = FileManager.ReadAllBytes(diskCachePath);
									preprocessed = true;
								}
								else
								{
									hadError = true;
									errorText = "Missing cache meta file " + text2;
								}
							}
							catch (Exception ex3)
							{
								LogUtil.LogError("Exception during cache file read " + ex3);
								hadError = true;
								errorText = ex3.ToString();
							}
						}
						else
						{
							try
							{
								//从var包中加载图片
								using (FileEntryStream fileEntryStream = FileManager.OpenStream(imgPath))
								{
									Stream stream = fileEntryStream.Stream;
									ProcessFromStream(stream);
								}
							}
							catch (Exception ex4)
							{
								hadError = true;
								LogUtil.LogError("Exception " + ex4 + " " + imgPath);
								errorText = ex4.ToString();
							}
						}
					}
					//else
					//{
					//	hadError = true;
					//	errorText = "Path " + imgPath + " is not valid";
					//}
				}
				else
				{
					finished = true;
				}
				processed = true;
			}

			protected bool IsPowerOfTwo(uint x)
			{
				return x != 0 && (x & (x - 1)) == 0;
			}

			public void Finish()
			{
				if (webRequest != null)
				{
					webRequest.Dispose();
					webRequestData = null;
					webRequest = null;
				}
				if (hadError || finished)
				{
					return;
				}
				bool flag = (!createMipMaps || !compress || (IsPowerOfTwo((uint)width) && IsPowerOfTwo((uint)height))) && compress;
				CreateTexture();
				if (preprocessed)
				{
					try
					{
						tex.LoadRawTextureData(raw);
					}
					catch
					{
						UnityEngine.Object.Destroy(tex);
						tex = null;
						createMipMaps = false;
						CreateTexture();
						tex.LoadRawTextureData(raw);
					}
					tex.Apply(false);
					if (compress && textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
					{
						tex.Compress(true);
					}
				}
				else if (tex.format == TextureFormat.DXT1 || tex.format == TextureFormat.DXT5)
				{
					Texture2D texture2D = new Texture2D(width, height, textureFormat, createMipMaps, linear);
					texture2D.LoadRawTextureData(raw);
					texture2D.Apply();
					texture2D.Compress(true);
					byte[] rawTextureData = texture2D.GetRawTextureData();
					tex.LoadRawTextureData(rawTextureData);
					tex.Apply();
					UnityEngine.Object.Destroy(texture2D);
				}
				else
				{
					tex.LoadRawTextureData(raw);
					tex.Apply();
					if (flag)
					{
						tex.Compress(true);
					}
					if (MVR.FileManagement.CacheManager.CachingEnabled)
					{
						string text = ((!Regex.IsMatch(imgPath, "^http")) ? GetDiskCachePath() : GetWebCachePath());
						if (text != null && !FileManager.FileExists(text))
						{
							try
							{
								JSONClass jSONClass = new JSONClass();
								jSONClass["type"] = "image";
								jSONClass["width"].AsInt = tex.width;
								jSONClass["height"].AsInt = tex.height;
								jSONClass["format"] = tex.format.ToString();
								string contents = jSONClass.ToString(string.Empty);
								byte[] rawTextureData2 = tex.GetRawTextureData();
								File.WriteAllText(text + "meta", contents);
								File.WriteAllBytes(text, rawTextureData2);
							}
							catch (Exception ex)
							{
								LogUtil.LogError("Exception during caching " + ex);
								hadError = true;
								errorText = "Exception during caching of " + imgPath + ": " + ex;
							}
						}
					}
				}
				finished = true;
			}

			public void DoCallback()
			{
				if (rawImageToLoad != null)
				{
					rawImageToLoad.texture = tex;
				}
				if (callback != null)
				{
					callback(this);
					callback = null;
				}
			}
		}

		protected class ImageLoaderTaskInfo
		{
			public string name;

			public AutoResetEvent resetEvent;

			public Thread thread;

			public volatile bool working;

			public volatile bool kill;
		}

		public static var_browser.CustomImageLoaderThreaded singleton;

		public GameObject progressHUD;

		public Slider progressSlider;

		public Text progressText;

		protected ImageLoaderTaskInfo imageLoaderTask;

		protected bool _threadsRunning;

		protected Dictionary<string, Texture2D> thumbnailCache;

		protected Dictionary<string, Texture2D> textureCache;

		protected Dictionary<string, Texture2D> immediateTextureCache;

		protected Dictionary<Texture2D, bool> textureTrackedCache;

		protected Dictionary<Texture2D, int> textureUseCount;

		protected volatile LinkedList<QueuedImage> queuedImages;

		protected int numRealQueuedImages;

		protected int progress;

		protected int progressMax;

		protected AsyncFlag loadFlag;

		protected void MTTask(object info)
		{
			ImageLoaderTaskInfo imageLoaderTaskInfo = (ImageLoaderTaskInfo)info;
			while (_threadsRunning)
			{
				imageLoaderTaskInfo.resetEvent.WaitOne(-1, true);
				if (imageLoaderTaskInfo.kill)
				{
					break;
				}
				ProcessImageQueueThreaded();
				imageLoaderTaskInfo.working = false;
			}
		}

		protected void StopThreads()
		{
			_threadsRunning = false;
			if (imageLoaderTask != null)
			{
				imageLoaderTask.kill = true;
				imageLoaderTask.resetEvent.Set();
				while (imageLoaderTask.thread.IsAlive)
				{
				}
				imageLoaderTask = null;
			}
		}

		protected void StartThreads()
		{
			if (!_threadsRunning)
			{
				_threadsRunning = true;
				imageLoaderTask = new ImageLoaderTaskInfo();
				imageLoaderTask.name = "ImageLoaderTask";
				imageLoaderTask.resetEvent = new AutoResetEvent(false);
				imageLoaderTask.thread = new Thread(MTTask);
				imageLoaderTask.thread.Priority = System.Threading.ThreadPriority.Normal;
				imageLoaderTask.thread.Start(imageLoaderTask);
			}
		}

		public bool RegisterTextureUse(Texture2D tex)
		{
			if (textureTrackedCache.ContainsKey(tex))
			{
				int value = 0;
				if (textureUseCount.TryGetValue(tex, out value))
				{
					textureUseCount.Remove(tex);
				}
				value++;
				textureUseCount.Add(tex, value);
				return true;
			}
			return false;
		}

		public bool DeregisterTextureUse(Texture2D tex)
		{
			int value = 0;
			if (textureUseCount.TryGetValue(tex, out value))
			{
				textureUseCount.Remove(tex);
				value--;
				if (value > 0)
				{
					textureUseCount.Add(tex, value);
				}
				else
				{
					textureUseCount.Remove(tex);
					textureCache.Remove(tex.name);
					textureTrackedCache.Remove(tex);
					UnityEngine.Object.Destroy(tex);
				}
				return true;
			}
			return false;
		}

		public void ReportOnTextures()
		{
			int num = 0;
			if (textureCache != null)
			{
				foreach (Texture2D value2 in textureCache.Values)
				{
					num++;
					int value = 0;
					if (textureUseCount.TryGetValue(value2, out value))
					{
						//SuperController.LogMessage("Texture " + value2.name + " is in use " + value + " times");
					}
				}
			}
			//SuperController.LogMessage("Using " + num + " textures");
		}

		public void PurgeAllTextures()
		{
			if (textureCache == null)
			{
				return;
			}
			foreach (Texture2D value in textureCache.Values)
			{
				UnityEngine.Object.Destroy(value);
			}
			textureUseCount.Clear();
			textureCache.Clear();
			textureTrackedCache.Clear();
		}

		public void PurgeAllImmediateTextures()
		{
			if (immediateTextureCache == null)
			{
				return;
			}
			foreach (Texture2D value in immediateTextureCache.Values)
			{
				UnityEngine.Object.Destroy(value);
			}
			immediateTextureCache.Clear();
		}

		public void ClearCacheThumbnail(string imgPath)
		{
			Texture2D value;
			if (thumbnailCache != null && thumbnailCache.TryGetValue(imgPath, out value))
			{
				thumbnailCache.Remove(imgPath);
				UnityEngine.Object.Destroy(value);
			}
		}

		protected void ProcessImageQueueThreaded()
		{
			if (queuedImages != null && queuedImages.Count > 0)
			{
				QueuedImage value = queuedImages.First.Value;
				value.Process();
			}
		}

		public Texture2D GetCachedThumbnail(string path)
		{
			Texture2D value;
			if (thumbnailCache != null && thumbnailCache.TryGetValue(path, out value))
			{
				return value;
			}
			return null;
		}

		public void QueueImage(QueuedImage qi)
		{
			if (queuedImages != null)
			{
				queuedImages.AddLast(qi);
			}
			numRealQueuedImages++;
			progressMax++;
		}

		public void QueueThumbnail(QueuedImage qi)
		{
			qi.isThumbnail = true;
			if (queuedImages != null)
			{
				queuedImages.AddLast(qi);
			}
		}

		public void QueueThumbnailImmediate(QueuedImage qi)
		{
			qi.isThumbnail = true;
			if (queuedImages != null)
			{
				if (queuedImages.Count > 0)
				{
					LinkedListNode<QueuedImage> first = queuedImages.First;
					queuedImages.AddAfter(first, qi);
				}
				else
				{
					queuedImages.AddLast(qi);
				}
			}
		}

		public void ProcessImageImmediate(QueuedImage qi)
		{
			Texture2D value;
			if (!qi.skipCache && immediateTextureCache != null && immediateTextureCache.TryGetValue(qi.cacheSignature, out value))
			{
				UseCachedTex(qi, value);
			}
			qi.Process();
			qi.Finish();
			if (!qi.skipCache && !immediateTextureCache.ContainsKey(qi.cacheSignature) && qi.tex != null)
			{
				immediateTextureCache.Add(qi.cacheSignature, qi.tex);
			}
		}

		protected void PostProcessImageQueue()
		{
			if (queuedImages == null || queuedImages.Count <= 0)
			{
				return;
			}
			QueuedImage value = queuedImages.First.Value;
			if (value.processed)
			{
				queuedImages.RemoveFirst();
				if (!value.isThumbnail)
				{
					progress++;
					numRealQueuedImages--;
					if (numRealQueuedImages == 0)
					{
						progress = 0;
						progressMax = 0;
						if (progressHUD != null)
						{
							progressHUD.SetActive(false);
						}
					}
					else
					{
						if (progressHUD != null)
						{
							progressHUD.SetActive(true);
						}
						if (progressSlider != null)
						{
							progressSlider.maxValue = progressMax;
							progressSlider.value = progress;
						}
					}
				}
				value.Finish();
				if (!value.skipCache && value.imgPath != null && value.imgPath != "NULL")
				{
					if (value.isThumbnail)
					{
						if (!thumbnailCache.ContainsKey(value.imgPath) && value.tex != null)
						{
							thumbnailCache.Add(value.imgPath, value.tex);
						}
					}
					else if (!textureCache.ContainsKey(value.cacheSignature) && value.tex != null)
					{
						textureCache.Add(value.cacheSignature, value.tex);
						textureTrackedCache.Add(value.tex, true);
					}
				}
				value.DoCallback();
			}
			if (numRealQueuedImages != 0)
			{
				if (loadFlag == null)
				{
					loadFlag = new AsyncFlag("ImageLoader");
					//SuperController.singleton.SetLoadingIconFlag(loadFlag);
				}
			}
			else if (loadFlag != null)
			{
				loadFlag.Raise();
				loadFlag = null;
			}
		}

		protected void UseCachedTex(QueuedImage qi, Texture2D tex)
		{
			qi.tex = tex;
			if (qi.forceReload)
			{
				qi.width = tex.width;
				qi.height = tex.height;
				qi.setSize = true;
				qi.fillBackground = false;
			}
			else
			{
				qi.processed = true;
				qi.finished = true;
			}
		}

		protected void RemoveCanceledImages()
		{
			if (queuedImages != null)
			{
				while (queuedImages.Count > 0 && queuedImages.First.Value.cancel)
				{
					queuedImages.RemoveFirst();
				}
			}
		}

		protected void PreprocessImageQueue()
		{
			RemoveCanceledImages();
			if (queuedImages == null || queuedImages.Count <= 0)
			{
				return;
			}
			QueuedImage value = queuedImages.First.Value;
			if (value == null)
			{
				return;
			}
			if (!value.skipCache && value.imgPath != null && value.imgPath != "NULL")
			{
				Texture2D value2;
				if (value.isThumbnail)
				{
					if (thumbnailCache != null && thumbnailCache.TryGetValue(value.imgPath, out value2))
					{
						if (value2 == null)
						{
							LogUtil.LogError("Trying to use cached texture at " + value.imgPath + " after it has been destroyed");
							thumbnailCache.Remove(value.imgPath);
						}
						else
						{
							UseCachedTex(value, value2);
						}
					}
				}
				else if (textureCache != null && textureCache.TryGetValue(value.cacheSignature, out value2))
				{
					if (value2 == null)
					{
						LogUtil.LogError("Trying to use cached texture at " + value.imgPath + " after it has been destroyed");
						textureCache.Remove(value.cacheSignature);
						textureTrackedCache.Remove(value2);
					}
					else
					{
						UseCachedTex(value, value2);
					}
				}
			}
			if (!value.processed && value.imgPath != null && Regex.IsMatch(value.imgPath, "^http"))
			{
				if (MVR.FileManagement.CacheManager.CachingEnabled && value.WebCachePathExists())
				{
					value.useWebCache = true;
				}
				else
				{
					if (value.webRequest == null)
					{
						value.webRequest = UnityWebRequest.Get(value.imgPath);
						value.webRequest.SendWebRequest();
					}
					if (value.webRequest.isDone)
					{
						if (!value.webRequest.isNetworkError)
						{
							if (value.webRequest.responseCode == 200)
							{
								value.webRequestData = value.webRequest.downloadHandler.data;
								value.webRequestDone = true;
							}
							else
							{
								value.webRequestHadError = true;
								value.webRequestDone = true;
								value.hadError = true;
								value.errorText = "Error " + value.webRequest.responseCode;
							}
						}
						else
						{
							value.webRequestHadError = true;
							value.webRequestDone = true;
							value.hadError = true;
							value.errorText = value.webRequest.error;
						}
					}
				}
			}
			if (!value.isThumbnail && progressText != null)
			{
				progressText.text = "[" + progress + "/" + progressMax + "] " + value.imgPath;
			}
		}

		private void Update()
		{
			StartThreads();
			if (!imageLoaderTask.working)
			{
				PostProcessImageQueue();
				if (queuedImages != null && queuedImages.Count > 0)
				{
					PreprocessImageQueue();
					imageLoaderTask.working = true;
					imageLoaderTask.resetEvent.Set();
				}
			}
		}

		public void OnDestroy()
		{
			if (Application.isPlaying)
			{
				StopThreads();
			}
			if (loadFlag != null)
			{
				loadFlag.Raise();
			}
			PurgeAllTextures();
			PurgeAllImmediateTextures();
		}

		protected void OnApplicationQuit()
		{
			if (Application.isPlaying)
			{
				StopThreads();
			}
		}

		private void Awake()
		{
			singleton = this;
			immediateTextureCache = new Dictionary<string, Texture2D>();
			textureCache = new Dictionary<string, Texture2D>();
			textureTrackedCache = new Dictionary<Texture2D, bool>();
			thumbnailCache = new Dictionary<string, Texture2D>();
			textureUseCount = new Dictionary<Texture2D, int>();
			queuedImages = new LinkedList<QueuedImage>();
		}
	}

}

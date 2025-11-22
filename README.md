# Infinite World System - Hệ thống Thế giới Mở Vô Tận

[![Video Demo](https://img.youtube.com/vi/FApLgq2DaqA/0.jpg)](https://youtu.be/FApLgq2DaqA)

**[🎥 Xem Video Demo](https://youtu.be/FApLgq2DaqA)**

Hệ thống này cho phép bạn tạo game thế giới mở vô tận với các tính năng:

## 🎯 Tính năng chính

### 1. **Chunk System**
- Thế giới được chia thành các chunk (i, j)
- Mỗi chunk có kích thước cố định (mặc định 50x50 units)
- Chỉ load chunks trong vùng quan sát của player (view distance)
- Tự động unload chunks khi player di chuyển xa

### 2. **Procedural Generation với Seed**
- Sử dụng world seed để tạo nội dung nhất quán
- Cùng seed + vị trí sẽ luôn tạo ra cùng kết quả
- **TỰ ĐỘNG sinh terrain mesh** từ multi-octave Perlin Noise
- Heightmap tự nhiên với 3 layers (base, medium, fine details)
- Vertex colors theo độ cao (valley → hills → mountains)
- Spawn objects (trees, rocks, props) dựa trên noise threshold

### 3. **Floating Origin**
- Giải quyết vấn đề floating point precision
- Khi player di chuyển xa origin (>1000 units), tự động shift toàn bộ thế giới
- Player luôn ở gần Vector3.zero trong Unity space
- Logic game vẫn track tọa độ thế giới thực

### 4. **World Coordinate System**
- Hệ thống tọa độ logic tách biệt với Unity world position
- ChunkIndex (Vector2Int) + LocalPosition (Vector2)
- Dễ dàng chuyển đổi giữa logic và render coordinates

## 📁 Cấu trúc Files

```
Assets/Scripts/InfiniteWorld/
├── WorldCoordinate.cs              - Hệ thống tọa độ logic
├── ChunkData.cs                    - Dữ liệu của một chunk
├── ChunkManager.cs                 - Quản lý load/unload chunks
├── WorldGenerator.cs               - Procedural generation với seed
├── ProceduralTerrainGenerator.cs   - Tự động sinh terrain mesh ✨
├── FloatingOrigin.cs               - Xử lý floating point precision
├── PlayerController.cs             - Controller mẫu để test
└── InfiniteWorldSetup.cs           - Helper setup nhanh
```

## 🚀 Cách sử dụng

### Setup tự động (Nhanh nhất):

1. Tạo Empty GameObject trong scene
2. Attach component `InfiniteWorldSetup`
3. Click chuột phải vào component > **Setup Infinite World**
4. Done! Hệ thống sẽ tự động tạo Player và World System

### Setup thủ công:

1. **Tạo Player:**
   - Tạo GameObject với tag "Player"
   - Add `CharacterController`
   - Add `PlayerController` component

2. **Tạo World System:**
   - Tạo Empty GameObject tên "WorldSystem"
   - Add `WorldGenerator` component
   - Add `ChunkManager` component
   - Add `FloatingOrigin` component
   - Assign references:
     - Player transform
     - WorldGenerator reference
     - ChunkManager reference

3. **Configure Settings:**
   - **ChunkManager:**
     - Chunk Size: 50 (kích thước mỗi chunk)
     - View Distance: 2 (load chunks trong phạm vi -2 đến 2)
   - **WorldGenerator:**
     - World Seed: 12345 (số bất kỳ)
     - Noise Scale: 0.1
     - Height Multiplier: 10
   - **FloatingOrigin:**
     - Threshold: 1000 (khoảng cách kích hoạt origin shift)

## 🎮 Điều khiển

- **WASD**: Di chuyển
- **Shift**: Sprint
- **Q/E hoặc Right Mouse**: Xoay camera
- **Space**: Nhảy (nếu có)

## 🔧 Tùy chỉnh

### Tự sinh terrain (Procedural):

**Mặc định hệ thống TỰ ĐỘNG sinh terrain mesh**, không cần prefabs!

1. Trong `WorldGenerator`:
   - Bật `Use Procedural Terrain` ✅ (mặc định)
   - Điều chỉnh settings trong `ProceduralTerrainGenerator`:
     - `Terrain Resolution`: Độ chi tiết mesh (20 = 400 vertices)
     - `Height Multiplier`: Độ cao tối đa
     - `Noise Scale`: Tỷ lệ noise (nhỏ = mượt, lớn = gồ ghề)
   - Tùy chỉnh màu sắc:
     - `Deep Color`: Màu vùng thấp (valley, đồng bằng)
     - `Mid Color`: Màu vùng trung bình (đồi)
     - `High Color`: Màu vùng cao (núi, đá)

2. **Nếu muốn dùng prefabs** thay vì tự sinh:
   - Tắt `Use Procedural Terrain` trong `WorldGenerator`
   - Assign prefabs vào `Terrain Prefabs`

### Thêm objects (trees, rocks, props):

1. Tạo prefabs cho objects
2. Assign vào `WorldGenerator`:
   - `Object Prefabs`: Mảng prefabs cho trees, rocks, etc.
   - `Objects Per Chunk`: Số objects tối đa mỗi chunk
   - `Object Spawn Threshold`: Ngưỡng spawn (0-1)

### Thay đổi thuật toán generation:

Edit method `GenerateChunk()` trong `WorldGenerator.cs`:

```csharp
public ChunkData GenerateChunk(Vector2Int chunkIndex, float chunkSize, Transform parent)
{
    // Your custom generation logic here
    // Sử dụng GetSeededInt(), GetSeededFloat() để đảm bảo consistency
}
```

### Tùy chỉnh nâng cao terrain:

Xem file `Examples_TerrainCustomization.cs` để biết cách:

1. **Tạo nhiều biomes** (Desert, Forest, Snow, etc):
```csharp
// Sử dụng BiomeTerrainGenerator
// Mỗi biome có màu, height scale, noise scale riêng
```

2. **Thêm caves/động** (3D Perlin Noise):
```csharp
// Sử dụng CaveTerrainGenerator
// Tạo caves underground với threshold
```

3. **Thêm water/ocean**:
```csharp
// Sử dụng WaterTerrainGenerator
// Tự động tạo water plane ở vùng thấp
```

### Tạo custom terrain generator:

Kế thừa từ `ProceduralTerrainGenerator`:

```csharp
public class MyTerrainGenerator : ProceduralTerrainGenerator
{
    // Override methods để custom generation logic
    // Hoặc tạo methods mới cho terrain types đặc biệt
}
```

## 📊 Performance Tips

1. **Chunk Size**: 
   - Nhỏ hơn = load/unload thường xuyên hơn, CPU cao
   - Lớn hơn = ít load/unload, nhưng mỗi chunk nặng hơn
   - Recommended: 50-100 units

2. **View Distance**:
   - Thấp hơn = ít chunks, performance tốt hơn
   - Cao hơn = nhiều chunks, thế giới rộng hơn
   - Recommended: 2-3 cho mobile, 3-5 cho PC

3. **Update Interval**:
   - ChunkManager update mỗi 0.5s (có thể điều chỉnh)
   - Tăng nếu muốn performance tốt hơn
   - Giảm nếu muốn responsive hơn

4. **Object Pooling**:
   - Implement object pooling thay vì Instantiate/Destroy
   - Giảm garbage collection và lag spikes

## 🐛 Debug

### Visualize chunks:
- Enable `Show Debug Gizmos` trong ChunkManager
- Scene view sẽ hiển thị:
  - Wire cubes xanh = Loaded chunks
  - Wire cube vàng = View distance range

### Debug UI:
- FloatingOrigin hiển thị:
  - Unity Position vs World Position
  - Shift count và total offset
- PlayerController hiển thị:
  - Current chunk index
  - Number of loaded chunks
  - Coordinate information

## 🔮 Mở rộng

### Save/Load System:

```csharp
public void SaveWorld()
{
    // Lưu world seed
    // Lưu modified chunks (chunks đã thay đổi khỏi procedural gen)
}

public void LoadWorld()
{
    // Load seed
    // Load modified chunks
    // Regenerate các chunks còn lại từ seed
}
```

### Multiplayer:

```csharp
// Mỗi player có ChunkManager riêng
// Server track modified chunks (build, destroy)
// Sync seed và modifications giữa clients
```

### LOD (Level of Detail):

```csharp
// Generate chunks với detail khác nhau dựa trên khoảng cách
// Far chunks: Low poly, simple objects
// Near chunks: High poly, detailed objects
```

## 📝 Lưu ý quan trọng

1. **Floating Origin** sẽ shift TẤT CẢ objects trong scene. Đảm bảo UI và các persistent objects không bị ảnh hưởng.

2. **Seed Consistency**: Đừng thay đổi thuật toán generation sau khi đã release, nếu không world sẽ khác.

3. **Chunk Boundaries**: Cẩn thận với objects nằm ở biên chunk, có thể bị cắt khi unload.

4. **Physics**: Rigidbodies sẽ được shift cùng, nhưng có thể có glitch nhỏ khi origin shift.

## 🎓 Tham khảo thêm

- [Unity Floating Origin](https://docs.unity3d.com/Manual/PositionAndOrientation.html)
- [Procedural Generation with Perlin Noise](https://catlikecoding.com/unity/tutorials/noise/)
- [Minecraft-style Chunk System](https://gamedev.stackexchange.com/questions/tagged/minecraft)

---

**Good luck với infinite world game của bạn! 🎮✨**

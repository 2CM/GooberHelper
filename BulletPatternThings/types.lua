-- the code to generate this can be found in Helpers/LuaHelper.cs

---@type Bullet
Bullet = require("#Celeste.Mod.GooberHelper.Entities.Bullet")

---@class Bullet
---@field Parent BulletActivator
---@field Velocity Vector2
---@field Acceleration Vector2
---@field Color Color
---@field Texture string
---@field Scale number
---@field Effect string
---@field Additive boolean
---@field LowResolution boolean
---@field Rotation number
---@field ColliderRadius number
---@field RotationMode BulletRotationMode
---@field PlayerCollider PlayerCollider
---@field ActualPosition Vector2
---@field Position Vector2
---@field Depth number
---@overload fun(parent: BulletActivator, template: BulletTemplate, position: Vector2?, velocity: Vector2?, acceleration: Vector2?, color: Color?, texture: string, scale: number?, effect: string, additive: unknown, lowResolution: unknown, rotation: number?, colliderRadius: number?): Bullet
local Bullet_ = {}

---@return nil
---@param key string
---@param to unknown
---@param time number
---@param easer Easer
function Bullet_:InterpolateValue(key, to, time, easer) return {} end

---@return nil
---@param lowResolution boolean
---@param effectName string
---@param additive boolean
function Bullet_.BeginRender(lowResolution, effectName, additive) return {} end

---@return nil
---@param lowResolution boolean
function Bullet_.EndRender(lowResolution) return {} end

---@return nil
function Bullet_:RemoveSelf() return {} end


---@type BulletActivator
BulletActivator = require("#Celeste.Mod.GooberHelper.Entities.BulletActivator")

---@class BulletActivator
---@field BulletFieldCenter Vector2
---@field Activated boolean
---@field ShaderPath string
---@field Depth number
---@overload fun(data: EntityData, offset: Vector2): BulletActivator
local BulletActivator_ = {}

---@return BetterCoroutine
---@param coroutine LuaCoroutine
function BulletActivator_:AddLuaCoroutine(coroutine) return {} end

---@return boolean
function BulletActivator_:CheckFlags() return false end

---@return nil
function BulletActivator_:Activate() return {} end

---@return nil
function BulletActivator_.CmdReloadLua() return {} end

---@return nil
function BulletActivator_:RemoveSelf() return {} end


---@type BulletTemplate
BulletTemplate = require("#Celeste.Mod.GooberHelper.BulletTemplate")

---@class BulletTemplate
---@field Velocity Vector2?
---@field Acceleration Vector2?
---@field Color Color?
---@field Texture string
---@field Scale number?
---@field Effect string
---@field Additive boolean?
---@field LowResolution boolean?
---@field Rotation number?
---@field ColliderRadius number?
---@overload fun(velocity: Vector2?, acceleration: Vector2?, color: Color?, texture: string, scale: number?, effect: string, additive: unknown, lowResolution: unknown, rotation: number?, colliderRadius: number?): BulletTemplate
---@operator add(BulletTemplate): BulletTemplate
local BulletTemplate_ = {}

---@return nil
---@param bullet Bullet
function BulletTemplate_:ApplyToBullet(bullet) return {} end

---@return nil
---@param template BulletTemplate
function BulletTemplate_:ApplyToBulletTemplate(template) return {} end


---@type Vector2
Vector2 = require("#Microsoft.Xna.Framework.Vector2")

---@class Vector2
---@field X number
---@field Y number
---@field Zero Vector2
---@field One Vector2
---@field UnitX Vector2
---@field UnitY Vector2
---@overload fun(x: number, y: number): Vector2
---@overload fun(value: number): Vector2
---@operator unm(): Vector2
---@operator add(Vector2): Vector2
---@operator sub(Vector2): Vector2
---@operator mul(Vector2): Vector2
---@operator mul(number): Vector2
---@operator mul(Vector2): Vector2
---@operator div(Vector2): Vector2
---@operator div(number): Vector2
local Vector2_ = {}

---@return boolean
---@param other Vector2
function Vector2_:Equals(other) return false end

---@return number
function Vector2_:Length() return 0 end

---@return number
function Vector2_:LengthSquared() return 0 end

---@return nil
function Vector2_:Normalize() return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Add(value1, value2) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Vector2&
function Vector2_.Add(value1, value2, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
---@param value3 Vector2
---@param amount1 number
---@param amount2 number
function Vector2_.Barycentric(value1, value2, value3, amount1, amount2) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param value3 Vector2&
---@param amount1 number
---@param amount2 number
---@param result Vector2&
function Vector2_.Barycentric(value1, value2, value3, amount1, amount2, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
---@param value3 Vector2
---@param value4 Vector2
---@param amount number
function Vector2_.CatmullRom(value1, value2, value3, value4, amount) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param value3 Vector2&
---@param value4 Vector2&
---@param amount number
---@param result Vector2&
function Vector2_.CatmullRom(value1, value2, value3, value4, amount, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param min Vector2
---@param max Vector2
function Vector2_.Clamp(value1, min, max) return {} end

---@return nil
---@param value1 Vector2&
---@param min Vector2&
---@param max Vector2&
---@param result Vector2&
function Vector2_.Clamp(value1, min, max, result) return {} end

---@return number
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Distance(value1, value2) return 0 end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Single&
function Vector2_.Distance(value1, value2, result) return {} end

---@return number
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.DistanceSquared(value1, value2) return 0 end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Single&
function Vector2_.DistanceSquared(value1, value2, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Divide(value1, value2) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Vector2&
function Vector2_.Divide(value1, value2, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param divider number
function Vector2_.Divide(value1, divider) return {} end

---@return nil
---@param value1 Vector2&
---@param divider number
---@param result Vector2&
function Vector2_.Divide(value1, divider, result) return {} end

---@return number
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Dot(value1, value2) return 0 end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Single&
function Vector2_.Dot(value1, value2, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param tangent1 Vector2
---@param value2 Vector2
---@param tangent2 Vector2
---@param amount number
function Vector2_.Hermite(value1, tangent1, value2, tangent2, amount) return {} end

---@return nil
---@param value1 Vector2&
---@param tangent1 Vector2&
---@param value2 Vector2&
---@param tangent2 Vector2&
---@param amount number
---@param result Vector2&
function Vector2_.Hermite(value1, tangent1, value2, tangent2, amount, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
---@param amount number
function Vector2_.Lerp(value1, value2, amount) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param amount number
---@param result Vector2&
function Vector2_.Lerp(value1, value2, amount, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Max(value1, value2) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Vector2&
function Vector2_.Max(value1, value2, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Min(value1, value2) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Vector2&
function Vector2_.Min(value1, value2, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Multiply(value1, value2) return {} end

---@return Vector2
---@param value1 Vector2
---@param scaleFactor number
function Vector2_.Multiply(value1, scaleFactor) return {} end

---@return nil
---@param value1 Vector2&
---@param scaleFactor number
---@param result Vector2&
function Vector2_.Multiply(value1, scaleFactor, result) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Vector2&
function Vector2_.Multiply(value1, value2, result) return {} end

---@return Vector2
---@param value Vector2
function Vector2_.Negate(value) return {} end

---@return nil
---@param value Vector2&
---@param result Vector2&
function Vector2_.Negate(value, result) return {} end

---@return Vector2
---@param value Vector2
function Vector2_.Normalize(value) return {} end

---@return nil
---@param value Vector2&
---@param result Vector2&
function Vector2_.Normalize(value, result) return {} end

---@return Vector2
---@param vector Vector2
---@param normal Vector2
function Vector2_.Reflect(vector, normal) return {} end

---@return nil
---@param vector Vector2&
---@param normal Vector2&
---@param result Vector2&
function Vector2_.Reflect(vector, normal, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
---@param amount number
function Vector2_.SmoothStep(value1, value2, amount) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param amount number
---@param result Vector2&
function Vector2_.SmoothStep(value1, value2, amount, result) return {} end

---@return Vector2
---@param value1 Vector2
---@param value2 Vector2
function Vector2_.Subtract(value1, value2) return {} end

---@return nil
---@param value1 Vector2&
---@param value2 Vector2&
---@param result Vector2&
function Vector2_.Subtract(value1, value2, result) return {} end

---@return Vector2
---@param position Vector2
---@param matrix Matrix
function Vector2_.Transform(position, matrix) return {} end

---@return nil
---@param position Vector2&
---@param matrix Matrix&
---@param result Vector2&
function Vector2_.Transform(position, matrix, result) return {} end

---@return Vector2
---@param value Vector2
---@param rotation Quaternion
function Vector2_.Transform(value, rotation) return {} end

---@return nil
---@param value Vector2&
---@param rotation Quaternion&
---@param result Vector2&
function Vector2_.Transform(value, rotation, result) return {} end

---@return nil
---@param sourceArray Vector2[]
---@param matrix Matrix&
---@param destinationArray Vector2[]
function Vector2_.Transform(sourceArray, matrix, destinationArray) return {} end

---@return nil
---@param sourceArray Vector2[]
---@param sourceIndex number
---@param matrix Matrix&
---@param destinationArray Vector2[]
---@param destinationIndex number
---@param length number
function Vector2_.Transform(sourceArray, sourceIndex, matrix, destinationArray, destinationIndex, length) return {} end

---@return nil
---@param sourceArray Vector2[]
---@param rotation Quaternion&
---@param destinationArray Vector2[]
function Vector2_.Transform(sourceArray, rotation, destinationArray) return {} end

---@return nil
---@param sourceArray Vector2[]
---@param sourceIndex number
---@param rotation Quaternion&
---@param destinationArray Vector2[]
---@param destinationIndex number
---@param length number
function Vector2_.Transform(sourceArray, sourceIndex, rotation, destinationArray, destinationIndex, length) return {} end

---@return Vector2
---@param normal Vector2
---@param matrix Matrix
function Vector2_.TransformNormal(normal, matrix) return {} end

---@return nil
---@param normal Vector2&
---@param matrix Matrix&
---@param result Vector2&
function Vector2_.TransformNormal(normal, matrix, result) return {} end

---@return nil
---@param sourceArray Vector2[]
---@param matrix Matrix&
---@param destinationArray Vector2[]
function Vector2_.TransformNormal(sourceArray, matrix, destinationArray) return {} end

---@return nil
---@param sourceArray Vector2[]
---@param sourceIndex number
---@param matrix Matrix&
---@param destinationArray Vector2[]
---@param destinationIndex number
---@param length number
function Vector2_.TransformNormal(sourceArray, sourceIndex, matrix, destinationArray, destinationIndex, length) return {} end


---@type Vector3
Vector3 = require("#Microsoft.Xna.Framework.Vector3")

---@class Vector3
---@field X number
---@field Y number
---@field Z number
---@field Zero Vector3
---@field One Vector3
---@field UnitX Vector3
---@field UnitY Vector3
---@field UnitZ Vector3
---@field Up Vector3
---@field Down Vector3
---@field Right Vector3
---@field Left Vector3
---@field Forward Vector3
---@field Backward Vector3
---@overload fun(x: number, y: number, z: number): Vector3
---@overload fun(value: number): Vector3
---@overload fun(value: Vector2, z: number): Vector3
---@operator add(Vector3): Vector3
---@operator unm(): Vector3
---@operator sub(Vector3): Vector3
---@operator mul(Vector3): Vector3
---@operator mul(number): Vector3
---@operator mul(Vector3): Vector3
---@operator div(Vector3): Vector3
---@operator div(number): Vector3
local Vector3_ = {}

---@return boolean
---@param other Vector3
function Vector3_:Equals(other) return false end

---@return number
function Vector3_:Length() return 0 end

---@return number
function Vector3_:LengthSquared() return 0 end

---@return nil
function Vector3_:Normalize() return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
function Vector3_.Add(value1, value2) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Vector3&
function Vector3_.Add(value1, value2, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
---@param value3 Vector3
---@param amount1 number
---@param amount2 number
function Vector3_.Barycentric(value1, value2, value3, amount1, amount2) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param value3 Vector3&
---@param amount1 number
---@param amount2 number
---@param result Vector3&
function Vector3_.Barycentric(value1, value2, value3, amount1, amount2, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
---@param value3 Vector3
---@param value4 Vector3
---@param amount number
function Vector3_.CatmullRom(value1, value2, value3, value4, amount) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param value3 Vector3&
---@param value4 Vector3&
---@param amount number
---@param result Vector3&
function Vector3_.CatmullRom(value1, value2, value3, value4, amount, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param min Vector3
---@param max Vector3
function Vector3_.Clamp(value1, min, max) return {} end

---@return nil
---@param value1 Vector3&
---@param min Vector3&
---@param max Vector3&
---@param result Vector3&
function Vector3_.Clamp(value1, min, max, result) return {} end

---@return Vector3
---@param vector1 Vector3
---@param vector2 Vector3
function Vector3_.Cross(vector1, vector2) return {} end

---@return nil
---@param vector1 Vector3&
---@param vector2 Vector3&
---@param result Vector3&
function Vector3_.Cross(vector1, vector2, result) return {} end

---@return number
---@param vector1 Vector3
---@param vector2 Vector3
function Vector3_.Distance(vector1, vector2) return 0 end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Single&
function Vector3_.Distance(value1, value2, result) return {} end

---@return number
---@param value1 Vector3
---@param value2 Vector3
function Vector3_.DistanceSquared(value1, value2) return 0 end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Single&
function Vector3_.DistanceSquared(value1, value2, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
function Vector3_.Divide(value1, value2) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Vector3&
function Vector3_.Divide(value1, value2, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 number
function Vector3_.Divide(value1, value2) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 number
---@param result Vector3&
function Vector3_.Divide(value1, value2, result) return {} end

---@return number
---@param vector1 Vector3
---@param vector2 Vector3
function Vector3_.Dot(vector1, vector2) return 0 end

---@return nil
---@param vector1 Vector3&
---@param vector2 Vector3&
---@param result Single&
function Vector3_.Dot(vector1, vector2, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param tangent1 Vector3
---@param value2 Vector3
---@param tangent2 Vector3
---@param amount number
function Vector3_.Hermite(value1, tangent1, value2, tangent2, amount) return {} end

---@return nil
---@param value1 Vector3&
---@param tangent1 Vector3&
---@param value2 Vector3&
---@param tangent2 Vector3&
---@param amount number
---@param result Vector3&
function Vector3_.Hermite(value1, tangent1, value2, tangent2, amount, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
---@param amount number
function Vector3_.Lerp(value1, value2, amount) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param amount number
---@param result Vector3&
function Vector3_.Lerp(value1, value2, amount, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
function Vector3_.Max(value1, value2) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Vector3&
function Vector3_.Max(value1, value2, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
function Vector3_.Min(value1, value2) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Vector3&
function Vector3_.Min(value1, value2, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
function Vector3_.Multiply(value1, value2) return {} end

---@return Vector3
---@param value1 Vector3
---@param scaleFactor number
function Vector3_.Multiply(value1, scaleFactor) return {} end

---@return nil
---@param value1 Vector3&
---@param scaleFactor number
---@param result Vector3&
function Vector3_.Multiply(value1, scaleFactor, result) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Vector3&
function Vector3_.Multiply(value1, value2, result) return {} end

---@return Vector3
---@param value Vector3
function Vector3_.Negate(value) return {} end

---@return nil
---@param value Vector3&
---@param result Vector3&
function Vector3_.Negate(value, result) return {} end

---@return Vector3
---@param value Vector3
function Vector3_.Normalize(value) return {} end

---@return nil
---@param value Vector3&
---@param result Vector3&
function Vector3_.Normalize(value, result) return {} end

---@return Vector3
---@param vector Vector3
---@param normal Vector3
function Vector3_.Reflect(vector, normal) return {} end

---@return nil
---@param vector Vector3&
---@param normal Vector3&
---@param result Vector3&
function Vector3_.Reflect(vector, normal, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
---@param amount number
function Vector3_.SmoothStep(value1, value2, amount) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param amount number
---@param result Vector3&
function Vector3_.SmoothStep(value1, value2, amount, result) return {} end

---@return Vector3
---@param value1 Vector3
---@param value2 Vector3
function Vector3_.Subtract(value1, value2) return {} end

---@return nil
---@param value1 Vector3&
---@param value2 Vector3&
---@param result Vector3&
function Vector3_.Subtract(value1, value2, result) return {} end

---@return Vector3
---@param position Vector3
---@param matrix Matrix
function Vector3_.Transform(position, matrix) return {} end

---@return nil
---@param position Vector3&
---@param matrix Matrix&
---@param result Vector3&
function Vector3_.Transform(position, matrix, result) return {} end

---@return nil
---@param sourceArray Vector3[]
---@param matrix Matrix&
---@param destinationArray Vector3[]
function Vector3_.Transform(sourceArray, matrix, destinationArray) return {} end

---@return nil
---@param sourceArray Vector3[]
---@param sourceIndex number
---@param matrix Matrix&
---@param destinationArray Vector3[]
---@param destinationIndex number
---@param length number
function Vector3_.Transform(sourceArray, sourceIndex, matrix, destinationArray, destinationIndex, length) return {} end

---@return Vector3
---@param value Vector3
---@param rotation Quaternion
function Vector3_.Transform(value, rotation) return {} end

---@return nil
---@param value Vector3&
---@param rotation Quaternion&
---@param result Vector3&
function Vector3_.Transform(value, rotation, result) return {} end

---@return nil
---@param sourceArray Vector3[]
---@param rotation Quaternion&
---@param destinationArray Vector3[]
function Vector3_.Transform(sourceArray, rotation, destinationArray) return {} end

---@return nil
---@param sourceArray Vector3[]
---@param sourceIndex number
---@param rotation Quaternion&
---@param destinationArray Vector3[]
---@param destinationIndex number
---@param length number
function Vector3_.Transform(sourceArray, sourceIndex, rotation, destinationArray, destinationIndex, length) return {} end

---@return Vector3
---@param normal Vector3
---@param matrix Matrix
function Vector3_.TransformNormal(normal, matrix) return {} end

---@return nil
---@param normal Vector3&
---@param matrix Matrix&
---@param result Vector3&
function Vector3_.TransformNormal(normal, matrix, result) return {} end

---@return nil
---@param sourceArray Vector3[]
---@param matrix Matrix&
---@param destinationArray Vector3[]
function Vector3_.TransformNormal(sourceArray, matrix, destinationArray) return {} end

---@return nil
---@param sourceArray Vector3[]
---@param sourceIndex number
---@param matrix Matrix&
---@param destinationArray Vector3[]
---@param destinationIndex number
---@param length number
function Vector3_.TransformNormal(sourceArray, sourceIndex, matrix, destinationArray, destinationIndex, length) return {} end


---@type Vector4
Vector4 = require("#Microsoft.Xna.Framework.Vector4")

---@class Vector4
---@field X number
---@field Y number
---@field Z number
---@field W number
---@field Zero Vector4
---@field One Vector4
---@field UnitX Vector4
---@field UnitY Vector4
---@field UnitZ Vector4
---@field UnitW Vector4
---@overload fun(x: number, y: number, z: number, w: number): Vector4
---@overload fun(value: Vector2, z: number, w: number): Vector4
---@overload fun(value: Vector3, w: number): Vector4
---@overload fun(value: number): Vector4
---@operator unm(): Vector4
---@operator add(Vector4): Vector4
---@operator sub(Vector4): Vector4
---@operator mul(Vector4): Vector4
---@operator mul(number): Vector4
---@operator mul(Vector4): Vector4
---@operator div(Vector4): Vector4
---@operator div(number): Vector4
local Vector4_ = {}

---@return boolean
---@param other Vector4
function Vector4_:Equals(other) return false end

---@return number
function Vector4_:Length() return 0 end

---@return number
function Vector4_:LengthSquared() return 0 end

---@return nil
function Vector4_:Normalize() return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.Add(value1, value2) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Vector4&
function Vector4_.Add(value1, value2, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
---@param value3 Vector4
---@param amount1 number
---@param amount2 number
function Vector4_.Barycentric(value1, value2, value3, amount1, amount2) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param value3 Vector4&
---@param amount1 number
---@param amount2 number
---@param result Vector4&
function Vector4_.Barycentric(value1, value2, value3, amount1, amount2, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
---@param value3 Vector4
---@param value4 Vector4
---@param amount number
function Vector4_.CatmullRom(value1, value2, value3, value4, amount) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param value3 Vector4&
---@param value4 Vector4&
---@param amount number
---@param result Vector4&
function Vector4_.CatmullRom(value1, value2, value3, value4, amount, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param min Vector4
---@param max Vector4
function Vector4_.Clamp(value1, min, max) return {} end

---@return nil
---@param value1 Vector4&
---@param min Vector4&
---@param max Vector4&
---@param result Vector4&
function Vector4_.Clamp(value1, min, max, result) return {} end

---@return number
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.Distance(value1, value2) return 0 end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Single&
function Vector4_.Distance(value1, value2, result) return {} end

---@return number
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.DistanceSquared(value1, value2) return 0 end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Single&
function Vector4_.DistanceSquared(value1, value2, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.Divide(value1, value2) return {} end

---@return Vector4
---@param value1 Vector4
---@param divider number
function Vector4_.Divide(value1, divider) return {} end

---@return nil
---@param value1 Vector4&
---@param divider number
---@param result Vector4&
function Vector4_.Divide(value1, divider, result) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Vector4&
function Vector4_.Divide(value1, value2, result) return {} end

---@return number
---@param vector1 Vector4
---@param vector2 Vector4
function Vector4_.Dot(vector1, vector2) return 0 end

---@return nil
---@param vector1 Vector4&
---@param vector2 Vector4&
---@param result Single&
function Vector4_.Dot(vector1, vector2, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param tangent1 Vector4
---@param value2 Vector4
---@param tangent2 Vector4
---@param amount number
function Vector4_.Hermite(value1, tangent1, value2, tangent2, amount) return {} end

---@return nil
---@param value1 Vector4&
---@param tangent1 Vector4&
---@param value2 Vector4&
---@param tangent2 Vector4&
---@param amount number
---@param result Vector4&
function Vector4_.Hermite(value1, tangent1, value2, tangent2, amount, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
---@param amount number
function Vector4_.Lerp(value1, value2, amount) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param amount number
---@param result Vector4&
function Vector4_.Lerp(value1, value2, amount, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.Max(value1, value2) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Vector4&
function Vector4_.Max(value1, value2, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.Min(value1, value2) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Vector4&
function Vector4_.Min(value1, value2, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.Multiply(value1, value2) return {} end

---@return Vector4
---@param value1 Vector4
---@param scaleFactor number
function Vector4_.Multiply(value1, scaleFactor) return {} end

---@return nil
---@param value1 Vector4&
---@param scaleFactor number
---@param result Vector4&
function Vector4_.Multiply(value1, scaleFactor, result) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Vector4&
function Vector4_.Multiply(value1, value2, result) return {} end

---@return Vector4
---@param value Vector4
function Vector4_.Negate(value) return {} end

---@return nil
---@param value Vector4&
---@param result Vector4&
function Vector4_.Negate(value, result) return {} end

---@return Vector4
---@param vector Vector4
function Vector4_.Normalize(vector) return {} end

---@return nil
---@param vector Vector4&
---@param result Vector4&
function Vector4_.Normalize(vector, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
---@param amount number
function Vector4_.SmoothStep(value1, value2, amount) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param amount number
---@param result Vector4&
function Vector4_.SmoothStep(value1, value2, amount, result) return {} end

---@return Vector4
---@param value1 Vector4
---@param value2 Vector4
function Vector4_.Subtract(value1, value2) return {} end

---@return nil
---@param value1 Vector4&
---@param value2 Vector4&
---@param result Vector4&
function Vector4_.Subtract(value1, value2, result) return {} end

---@return Vector4
---@param position Vector2
---@param matrix Matrix
function Vector4_.Transform(position, matrix) return {} end

---@return Vector4
---@param position Vector3
---@param matrix Matrix
function Vector4_.Transform(position, matrix) return {} end

---@return Vector4
---@param vector Vector4
---@param matrix Matrix
function Vector4_.Transform(vector, matrix) return {} end

---@return nil
---@param position Vector2&
---@param matrix Matrix&
---@param result Vector4&
function Vector4_.Transform(position, matrix, result) return {} end

---@return nil
---@param position Vector3&
---@param matrix Matrix&
---@param result Vector4&
function Vector4_.Transform(position, matrix, result) return {} end

---@return nil
---@param vector Vector4&
---@param matrix Matrix&
---@param result Vector4&
function Vector4_.Transform(vector, matrix, result) return {} end

---@return nil
---@param sourceArray Vector4[]
---@param matrix Matrix&
---@param destinationArray Vector4[]
function Vector4_.Transform(sourceArray, matrix, destinationArray) return {} end

---@return nil
---@param sourceArray Vector4[]
---@param sourceIndex number
---@param matrix Matrix&
---@param destinationArray Vector4[]
---@param destinationIndex number
---@param length number
function Vector4_.Transform(sourceArray, sourceIndex, matrix, destinationArray, destinationIndex, length) return {} end

---@return Vector4
---@param value Vector2
---@param rotation Quaternion
function Vector4_.Transform(value, rotation) return {} end

---@return Vector4
---@param value Vector3
---@param rotation Quaternion
function Vector4_.Transform(value, rotation) return {} end

---@return Vector4
---@param value Vector4
---@param rotation Quaternion
function Vector4_.Transform(value, rotation) return {} end

---@return nil
---@param value Vector2&
---@param rotation Quaternion&
---@param result Vector4&
function Vector4_.Transform(value, rotation, result) return {} end

---@return nil
---@param value Vector3&
---@param rotation Quaternion&
---@param result Vector4&
function Vector4_.Transform(value, rotation, result) return {} end

---@return nil
---@param value Vector4&
---@param rotation Quaternion&
---@param result Vector4&
function Vector4_.Transform(value, rotation, result) return {} end

---@return nil
---@param sourceArray Vector4[]
---@param rotation Quaternion&
---@param destinationArray Vector4[]
function Vector4_.Transform(sourceArray, rotation, destinationArray) return {} end

---@return nil
---@param sourceArray Vector4[]
---@param sourceIndex number
---@param rotation Quaternion&
---@param destinationArray Vector4[]
---@param destinationIndex number
---@param length number
function Vector4_.Transform(sourceArray, sourceIndex, rotation, destinationArray, destinationIndex, length) return {} end


---@type Color
Color = require("#Microsoft.Xna.Framework.Color")

---@class Color
---@field B number
---@field G number
---@field R number
---@field A number
---@field PackedValue number
---@field Transparent Color
---@field AliceBlue Color
---@field AntiqueWhite Color
---@field Aqua Color
---@field Aquamarine Color
---@field Azure Color
---@field Beige Color
---@field Bisque Color
---@field Black Color
---@field BlanchedAlmond Color
---@field Blue Color
---@field BlueViolet Color
---@field Brown Color
---@field BurlyWood Color
---@field CadetBlue Color
---@field Chartreuse Color
---@field Chocolate Color
---@field Coral Color
---@field CornflowerBlue Color
---@field Cornsilk Color
---@field Crimson Color
---@field Cyan Color
---@field DarkBlue Color
---@field DarkCyan Color
---@field DarkGoldenrod Color
---@field DarkGray Color
---@field DarkGreen Color
---@field DarkKhaki Color
---@field DarkMagenta Color
---@field DarkOliveGreen Color
---@field DarkOrange Color
---@field DarkOrchid Color
---@field DarkRed Color
---@field DarkSalmon Color
---@field DarkSeaGreen Color
---@field DarkSlateBlue Color
---@field DarkSlateGray Color
---@field DarkTurquoise Color
---@field DarkViolet Color
---@field DeepPink Color
---@field DeepSkyBlue Color
---@field DimGray Color
---@field DodgerBlue Color
---@field Firebrick Color
---@field FloralWhite Color
---@field ForestGreen Color
---@field Fuchsia Color
---@field Gainsboro Color
---@field GhostWhite Color
---@field Gold Color
---@field Goldenrod Color
---@field Gray Color
---@field Green Color
---@field GreenYellow Color
---@field Honeydew Color
---@field HotPink Color
---@field IndianRed Color
---@field Indigo Color
---@field Ivory Color
---@field Khaki Color
---@field Lavender Color
---@field LavenderBlush Color
---@field LawnGreen Color
---@field LemonChiffon Color
---@field LightBlue Color
---@field LightCoral Color
---@field LightCyan Color
---@field LightGoldenrodYellow Color
---@field LightGray Color
---@field LightGreen Color
---@field LightPink Color
---@field LightSalmon Color
---@field LightSeaGreen Color
---@field LightSkyBlue Color
---@field LightSlateGray Color
---@field LightSteelBlue Color
---@field LightYellow Color
---@field Lime Color
---@field LimeGreen Color
---@field Linen Color
---@field Magenta Color
---@field Maroon Color
---@field MediumAquamarine Color
---@field MediumBlue Color
---@field MediumOrchid Color
---@field MediumPurple Color
---@field MediumSeaGreen Color
---@field MediumSlateBlue Color
---@field MediumSpringGreen Color
---@field MediumTurquoise Color
---@field MediumVioletRed Color
---@field MidnightBlue Color
---@field MintCream Color
---@field MistyRose Color
---@field Moccasin Color
---@field NavajoWhite Color
---@field Navy Color
---@field OldLace Color
---@field Olive Color
---@field OliveDrab Color
---@field Orange Color
---@field OrangeRed Color
---@field Orchid Color
---@field PaleGoldenrod Color
---@field PaleGreen Color
---@field PaleTurquoise Color
---@field PaleVioletRed Color
---@field PapayaWhip Color
---@field PeachPuff Color
---@field Peru Color
---@field Pink Color
---@field Plum Color
---@field PowderBlue Color
---@field Purple Color
---@field Red Color
---@field RosyBrown Color
---@field RoyalBlue Color
---@field SaddleBrown Color
---@field Salmon Color
---@field SandyBrown Color
---@field SeaGreen Color
---@field SeaShell Color
---@field Sienna Color
---@field Silver Color
---@field SkyBlue Color
---@field SlateBlue Color
---@field SlateGray Color
---@field Snow Color
---@field SpringGreen Color
---@field SteelBlue Color
---@field Tan Color
---@field Teal Color
---@field Thistle Color
---@field Tomato Color
---@field Turquoise Color
---@field Violet Color
---@field Wheat Color
---@field White Color
---@field WhiteSmoke Color
---@field Yellow Color
---@field YellowGreen Color
---@overload fun(color: Vector4): Color
---@overload fun(color: Vector3): Color
---@overload fun(r: number, g: number, b: number): Color
---@overload fun(r: number, g: number, b: number): Color
---@overload fun(r: number, g: number, b: number, alpha: number): Color
---@overload fun(r: number, g: number, b: number, alpha: number): Color
---@overload fun(color: Color, alpha: number): Color
---@overload fun(color: Color, alpha: number): Color
---@operator mul(number): Color
local Color_ = {}

---@return boolean
---@param other Color
function Color_:Equals(other) return false end

---@return Vector3
function Color_:ToVector3() return {} end

---@return Vector4
function Color_:ToVector4() return {} end

---@return Color
---@param value1 Color
---@param value2 Color
---@param amount number
function Color_.Lerp(value1, value2, amount) return {} end

---@return Color
---@param vector Vector4
function Color_.FromNonPremultiplied(vector) return {} end

---@return Color
---@param r number
---@param g number
---@param b number
---@param a number
function Color_.FromNonPremultiplied(r, g, b, a) return {} end

---@return Color
---@param value Color
---@param scale number
function Color_.Multiply(value, scale) return {} end


---@enum BulletRotationMode
_G.BulletRotationMode = {
	None = 0,
	Velocity = 1,
	PositionChange = 2,
}


---@type Ease
Ease = require("#Monocle.Ease")

---@class Ease
---@field Linear Easer
---@field SineIn Easer
---@field SineOut Easer
---@field SineInOut Easer
---@field QuadIn Easer
---@field QuadOut Easer
---@field QuadInOut Easer
---@field CubeIn Easer
---@field CubeOut Easer
---@field CubeInOut Easer
---@field QuintIn Easer
---@field QuintOut Easer
---@field QuintInOut Easer
---@field ExpoIn Easer
---@field ExpoOut Easer
---@field ExpoInOut Easer
---@field BackIn Easer
---@field BackOut Easer
---@field BackInOut Easer
---@field BigBackIn Easer
---@field BigBackOut Easer
---@field BigBackInOut Easer
---@field ElasticIn Easer
---@field ElasticOut Easer
---@field ElasticInOut Easer
---@field BounceIn Easer
---@field BounceOut Easer
---@field BounceInOut Easer
local Ease_ = {}

---@return Easer
---@param easer Easer
function Ease_.Invert(easer) return {} end

---@return Easer
---@param first Easer
---@param second Easer
function Ease_.Follow(first, second) return {} end

---@return number
---@param eased number
function Ease_.UpDown(eased) return 0 end



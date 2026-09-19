"""
TestGame - Original Protagonist Generator
Blender 4.x

Run from Blender's Scripting workspace:
    1. Open this file.
    2. Run Script.
    3. The script creates:
       Assets/Resources/PlayerCharacter/PlayerCharacter.fbx
       Assets/Resources/PlayerCharacter/PlayerCharacter.blend
       (the FBX is the runtime character asset)

The character is original and follows the project's art direction:
human-proportioned, lean athletic, asymmetric lightweight armor,
sealed visor, compact rear energy unit, Japanese-inspired energy sword.
"""

import bpy
import math
import os
from mathutils import Vector

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OUTPUT_DIR = os.path.join(ROOT, "Assets", "Resources", "PlayerCharacter")
FBX_PATH = os.path.join(OUTPUT_DIR, "PlayerCharacter.fbx")
BLEND_PATH = os.path.join(OUTPUT_DIR, "PlayerCharacter.blend")

# ---------- scene ----------

def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials,
                       bpy.data.cameras, bpy.data.lights, bpy.data.armatures):
        # Materials/meshes are harmless to retain, but clearing them makes repeated runs deterministic.
        pass

def mat(name, base, metallic=0.0, roughness=0.5, emission=None, emission_strength=0.0):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*base, 1.0)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.45
    if emission:
        bsdf.inputs["Emission Color"].default_value = (*emission, 1.0)
        bsdf.inputs["Emission Strength"].default_value = emission_strength
    return m

MAT_BODY = mat("InnerSuit", (0.018, 0.024, 0.038), 0.05, 0.62)
MAT_ARMOR = mat("GraphiteArmor", (0.055, 0.075, 0.105), 0.72, 0.30)
MAT_ARMOR_LIGHT = mat("BlueGrayArmor", (0.10, 0.125, 0.17), 0.65, 0.27)
MAT_METAL = mat("WarmMetal", (0.30, 0.20, 0.075), 0.82, 0.22)
MAT_CRIMSON = mat("CrimsonAccent", (0.32, 0.015, 0.045), 0.35, 0.28, (0.55, 0.01, 0.025), 1.6)
MAT_CYAN = mat("EnergyCyan", (0.005, 0.18, 0.23), 0.15, 0.18, (0.02, 0.75, 1.0), 8.0)
MAT_VISOR = mat("Visor", (0.003, 0.012, 0.018), 0.65, 0.12, (0.01, 0.22, 0.28), 1.5)
MAT_BLADE = mat("EnergyBlade", (0.02, 0.30, 0.38), 0.25, 0.12, (0.02, 0.85, 1.0), 10.0)

def smooth(obj):
    if obj.type == "MESH":
        for p in obj.data.polygons:
            p.use_smooth = True

def apply_mat(obj, material):
    obj.data.materials.append(material)
    smooth(obj)

def uv_sphere(name, loc, scale, material, segments=24, rings=16):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=loc)
    o = bpy.context.object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    apply_mat(o, material)
    return o

def capsule(name, loc, radius, depth, material, rotation=(0,0,0)):
    bpy.ops.mesh.primitive_capsule_add(radius=radius, depth=depth, location=loc, rotation=rotation)
    o = bpy.context.object
    o.name = name
    apply_mat(o, material)
    return o

def cube_bevel(name, loc, scale, material, bevel=0.04, rotation=(0,0,0)):
    bpy.ops.mesh.primitive_cube_add(location=loc, rotation=rotation)
    o = bpy.context.object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel_mod = o.modifiers.new("SoftEdges", "BEVEL")
    bevel_mod.width = bevel
    bevel_mod.segments = 3
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier=bevel_mod.name)
    apply_mat(o, material)
    return o

# ---------- character mesh ----------

def build_character():
    parts = []

    # Human silhouette: approximately 7.5 heads tall.
    parts += [
        capsule("Torso", (0,0,1.42), 0.30, 0.92, MAT_BODY),
        capsule("Neck", (0,0,2.02), 0.105, 0.18, MAT_BODY),
        uv_sphere("Pelvis", (0,0,0.78), (0.34,0.25,0.25), MAT_BODY),

        capsule("L_UpperArm", (-0.42,0,1.55), 0.105, 0.52, MAT_BODY, (0,0,-0.18)),
        capsule("R_UpperArm", (0.42,0,1.55), 0.105, 0.52, MAT_BODY, (0,0,0.18)),
        capsule("L_Forearm", (-0.52,0.01,1.12), 0.09, 0.50, MAT_BODY, (0,0,-0.10)),
        capsule("R_Forearm", (0.52,0.01,1.12), 0.09, 0.50, MAT_BODY, (0,0,0.10)),
        uv_sphere("L_Hand", (-0.55,0.03,0.78), (0.10,0.09,0.12), MAT_BODY),
        uv_sphere("R_Hand", (0.55,0.03,0.78), (0.10,0.09,0.12), MAT_BODY),

        capsule("L_Thigh", (-0.17,0,0.25), 0.135, 0.72, MAT_BODY),
        capsule("R_Thigh", (0.17,0,0.25), 0.135, 0.72, MAT_BODY),
        capsule("L_Shin", (-0.17,0,-0.52), 0.105, 0.78, MAT_BODY),
        capsule("R_Shin", (0.17,0,-0.52), 0.105, 0.78, MAT_BODY),
        uv_sphere("L_Foot", (-0.17,0.10,-1.02), (0.13,0.27,0.10), MAT_BODY),
        uv_sphere("R_Foot", (0.17,0.10,-1.02), (0.13,0.27,0.10), MAT_BODY),

        uv_sphere("Head", (0,0,2.36), (0.225,0.20,0.27), MAT_BODY),
    ]

    # Layered armor follows anatomy instead of replacing it.
    parts += [
        cube_bevel("ChestCore", (0,-0.205,1.56), (0.30,0.075,0.30), MAT_ARMOR, 0.055),
        cube_bevel("ChestLeft", (-0.22,-0.225,1.58), (0.17,0.045,0.21), MAT_ARMOR_LIGHT, 0.04, (0,0,-0.10)),
        cube_bevel("ChestRight", (0.22,-0.225,1.58), (0.18,0.05,0.22), MAT_ARMOR_LIGHT, 0.04, (0,0,0.10)),
        cube_bevel("RightShoulder", (0.51,0,1.76), (0.19,0.15,0.20), MAT_ARMOR, 0.06, (0,0,-0.12)),
        cube_bevel("LeftShoulder", (-0.47,0,1.74), (0.14,0.11,0.15), MAT_ARMOR_LIGHT, 0.045, (0,0,0.12)),
        cube_bevel("RightForearmGuard", (0.53,-0.02,1.12), (0.105,0.07,0.19), MAT_ARMOR, 0.035, (0.15,0.05,0)),
        cube_bevel("LeftForearmGuard", (-0.53,-0.02,1.12), (0.10,0.065,0.18), MAT_ARMOR_LIGHT, 0.035, (-0.15,-0.05,0)),
        cube_bevel("PelvisArmor", (0,-0.02,0.78), (0.34,0.12,0.19), MAT_ARMOR, 0.055),
        cube_bevel("RightHip", (0.34,-0.01,0.70), (0.13,0.11,0.16), MAT_CRIMSON, 0.035),
        cube_bevel("LeftKnee", (-0.17,-0.12,-0.13), (0.12,0.08,0.11), MAT_ARMOR_LIGHT, 0.035),
        cube_bevel("RightKnee", (0.17,-0.12,-0.13), (0.12,0.08,0.11), MAT_ARMOR_LIGHT, 0.035),
        cube_bevel("LeftShinArmor", (-0.17,-0.01,-0.52), (0.11,0.10,0.26), MAT_ARMOR, 0.04),
        cube_bevel("RightShinArmor", (0.17,-0.01,-0.52), (0.11,0.10,0.26), MAT_ARMOR, 0.04),
        cube_bevel("Collar", (0,0,2.00), (0.22,0.12,0.09), MAT_ARMOR, 0.04),
    ]

    # Helmet / visor / crest.
    parts += [
        uv_sphere("HelmetShell", (0,0,2.40), (0.255,0.225,0.285), MAT_ARMOR),
        cube_bevel("Visor", (0,-0.205,2.38), (0.17,0.018,0.035), MAT_VISOR, 0.015),
        cube_bevel("VisorGlow", (0,-0.225,2.385), (0.105,0.006,0.009), MAT_CYAN, 0.005),
        cube_bevel("HelmetCrest", (0,0.01,2.69), (0.035,0.055,0.19), MAT_CRIMSON, 0.02),
    ]

    # Rear reactor, kept close to the body.
    parts += [
        cube_bevel("RearEnergyHousing", (0,0.22,1.45), (0.13,0.10,0.33), MAT_ARMOR, 0.04),
        cube_bevel("RearEnergyCore", (0,0.335,1.45), (0.035,0.012,0.25), MAT_CYAN, 0.01),
        cube_bevel("RearFinL", (-0.16,0.19,1.52), (0.035,0.05,0.20), MAT_ARMOR_LIGHT, 0.02, (0.0,0.25,0)),
        cube_bevel("RearFinR", (0.16,0.19,1.52), (0.035,0.05,0.20), MAT_ARMOR_LIGHT, 0.02, (0.0,-0.25,0)),
    ]

    # Chest seal and energy conduits.
    parts += [
        cube_bevel("ChestEnergySeal", (0,-0.285,1.58), (0.025,0.008,0.16), MAT_CYAN, 0.008),
        cube_bevel("ChestCrimsonLine", (0.25,-0.285,1.64), (0.018,0.008,0.10), MAT_CRIMSON, 0.006),
        cube_bevel("BootGlowL", (-0.17,-0.14,-0.95), (0.07,0.018,0.018), MAT_CYAN, 0.008),
        cube_bevel("BootGlowR", (0.17,-0.14,-0.95), (0.07,0.018,0.018), MAT_CYAN, 0.008),
    ]

    # Sword: dark sheath and energy blade mounted at the waist.
    sheath = cube_bevel("SwordSheath", (-0.48,0.02,0.98), (0.055,0.055,0.48), MAT_BODY, 0.025, (0,0,-0.12))
    handle = cube_bevel("SwordHandle", (-0.46,-0.02,1.48), (0.045,0.045,0.18), MAT_ARMOR_LIGHT, 0.02, (0,0,-0.12))
    guard = cube_bevel("SwordGuard", (-0.46,-0.08,1.30), (0.12,0.025,0.04), MAT_METAL, 0.012)
    blade = cube_bevel("EnergyBlade", (-0.46,-0.03,1.95), (0.025,0.012,0.50), MAT_BLADE, 0.015)
    blade.rotation_euler[2] = math.radians(-6)
    parts += [sheath, handle, guard, blade]

    return parts

# ---------- rig ----------

BONE_DEFS = {
    "root": ((0,0,0), (0,0,0.35), None),
    "pelvis": ((0,0,0.35), (0,0,0.90), "root"),
    "spine": ((0,0,0.90), (0,0,1.45), "pelvis"),
    "chest": ((0,0,1.45), (0,0,1.95), "spine"),
    "neck": ((0,0,1.95), (0,0,2.12), "chest"),
    "head": ((0,0,2.12), (0,0,2.55), "neck"),
    "upper_arm.L": ((-0.05,0,1.82), (-0.48,0,1.48), "chest"),
    "forearm.L": ((-0.48,0,1.48), (-0.53,0,1.10), "upper_arm.L"),
    "hand.L": ((-0.53,0,1.10), (-0.55,0,0.78), "forearm.L"),
    "upper_arm.R": ((0.05,0,1.82), (0.48,0,1.48), "chest"),
    "forearm.R": ((0.48,0,1.48), (0.53,0,1.10), "upper_arm.R"),
    "hand.R": ((0.53,0,1.10), (0.55,0,0.78), "forearm.R"),
    "thigh.L": ((-0.10,0,0.75), (-0.17,0,-0.10), "pelvis"),
    "shin.L": ((-0.17,0,-0.10), (-0.17,0,-0.82), "thigh.L"),
    "foot.L": ((-0.17,0,-0.82), (-0.17,0.16,-1.02), "shin.L"),
    "thigh.R": ((0.10,0,0.75), (0.17,0,-0.10), "pelvis"),
    "shin.R": ((0.17,0,-0.10), (0.17,0,-0.82), "thigh.R"),
    "foot.R": ((0.17,0,-0.82), (0.17,0.16,-1.02), "shin.R"),
}

def make_rig():
    bpy.ops.object.armature_add(location=(0,0,0))
    arm = bpy.context.object
    arm.name = "PlayerArmature"
    arm.data.name = "PlayerArmature"
    arm.show_in_front = True

    bpy.ops.object.mode_set(mode="EDIT")
    eb = arm.data.edit_bones
    for b in list(eb):
        eb.remove(b)

    for name, (head, tail, parent) in BONE_DEFS.items():
        b = eb.new(name)
        b.head = head
        b.tail = tail
        if parent:
            b.parent = eb[parent]
            b.use_connect = False

    bpy.ops.object.mode_set(mode="POSE")
    return arm

def assign_rigid_parent(obj, arm, bone_name):
    obj.parent = arm
    obj.parent_type = "BONE"
    obj.parent_bone = bone_name
    obj.matrix_parent_inverse = arm.matrix_world.inverted()

def bind_parts(parts, arm):
    for obj in parts:
        n = obj.name.lower()
        if "head" in n or "visor" in n or "helmet" in n or "crest" in n:
            bone = "head"
        elif "upperarm" in n or "shoulder" in n:
            bone = "upper_arm.L" if n.startswith("l_") or ".l" in n or "left" in n else "upper_arm.R"
        elif "forearm" in n:
            bone = "forearm.L" if n.startswith("l_") or ".l" in n or "left" in n else "forearm.R"
        elif "hand" in n:
            bone = "hand.L" if n.startswith("l_") or ".l" in n or "left" in n else "hand.R"
        elif "thigh" in n:
            bone = "thigh.L" if n.startswith("l_") or ".l" in n or "left" in n else "thigh.R"
        elif "shin" in n or "knee" in n or "boot" in n:
            bone = "shin.L" if n.startswith("l_") or ".l" in n or "left" in n else "shin.R"
        elif "foot" in n:
            bone = "foot.L" if n.startswith("l_") or ".l" in n or "left" in n else "foot.R"
        elif "pelvis" in n or "hip" in n or "sword" in n:
            bone = "pelvis"
        elif "neck" in n or "collar" in n or "chest" in n or "torso" in n or "rear" in n:
            bone = "chest"
        else:
            bone = "root"
        assign_rigid_parent(obj, arm, bone)

# ---------- animation ----------

def key_pose(arm, frame, values):
    bpy.context.scene.frame_set(frame)
    for bone_name, (loc, rot) in values.items():
        pb = arm.pose.bones.get(bone_name)
        if not pb:
            continue
        pb.location = loc
        pb.rotation_mode = "XYZ"
        pb.rotation_euler = rot
        pb.keyframe_insert(data_path="location", frame=frame)
        pb.keyframe_insert(data_path="rotation_euler", frame=frame)

def action_idle(arm):
    arm.animation_data_create()
    action = bpy.data.actions.new("Idle")
    arm.animation_data.action = action
    key_pose(arm, 1, {})
    key_pose(arm, 30, {"spine":((0,0,0),(0,0,math.radians(-1.5))), "chest":((0,0,0),(0,0,math.radians(1.5)))})
    key_pose(arm, 60, {})
    return action

def action_run(arm):
    action = bpy.data.actions.new("Run")
    arm.animation_data.action = action
    key_pose(arm,1,{"thigh.L":((0,0,0),(math.radians(25),0,0)),"thigh.R":((0,0,0),(math.radians(-25),0,0)),"upper_arm.L":((0,0,0),(math.radians(-20),0,0)),"upper_arm.R":((0,0,0),(math.radians(20),0,0))})
    key_pose(arm,8,{"thigh.L":((0,0,0),(math.radians(-25),0,0)),"thigh.R":((0,0,0),(math.radians(25),0,0)),"upper_arm.L":((0,0,0),(math.radians(20),0,0)),"upper_arm.R":((0,0,0),(math.radians(-20),0,0))})
    key_pose(arm,16,{"thigh.L":((0,0,0),(math.radians(25),0,0)),"thigh.R":((0,0,0),(math.radians(-25),0,0)),"upper_arm.L":((0,0,0),(math.radians(-20),0,0)),"upper_arm.R":((0,0,0),(math.radians(20),0,0))})
    return action

def action_attack(arm, name, direction=1):
    action = bpy.data.actions.new(name)
    arm.animation_data.action = action
    key_pose(arm,1,{})
    key_pose(arm,7,{
        "spine":((0,0,0),(0,math.radians(-12*direction),math.radians(5*direction))),
        "chest":((0,0,0),(0,math.radians(-18*direction),math.radians(7*direction))),
        "upper_arm.R":((0,0,0),(math.radians(-55),math.radians(20*direction),math.radians(-20*direction))),
        "forearm.R":((0,0,0),(math.radians(-35),0,math.radians(15*direction))),
    })
    key_pose(arm,13,{
        "spine":((0,0,0),(0,math.radians(15*direction),math.radians(-5*direction))),
        "chest":((0,0,0),(0,math.radians(22*direction),math.radians(-7*direction))),
        "upper_arm.R":((0,0,0),(math.radians(65),math.radians(-25*direction),math.radians(30*direction))),
        "forearm.R":((0,0,0),(math.radians(55),0,math.radians(-20*direction))),
        "thigh.L":((0,0,0),(math.radians(-8),0,0)),
        "thigh.R":((0,0,0),(math.radians(8),0,0)),
    })
    key_pose(arm,22,{})
    return action

def action_dodge(arm):
    action = bpy.data.actions.new("Dodge")
    arm.animation_data.action = action
    key_pose(arm,1,{"spine":((0,0,0),(0,0,0))})
    key_pose(arm,5,{"spine":((0,0.05,0),(math.radians(-18),0,math.radians(-12))),"thigh.L":((0,0,0),(math.radians(12),0,0)),"thigh.R":((0,0,0),(math.radians(12),0,0))})
    key_pose(arm,12,{"spine":((0,0,0),(math.radians(8),0,math.radians(8)))})
    key_pose(arm,18,{})
    return action

def action_skill(arm, name):
    action = bpy.data.actions.new(name)
    arm.animation_data.action = action
    key_pose(arm,1,{})
    key_pose(arm,12,{"spine":((0,0,0),(0,math.radians(-25),0)),"upper_arm.R":((0,0,0),(math.radians(-70),math.radians(25),0)),"forearm.R":((0,0,0),(math.radians(-45),0,0))})
    key_pose(arm,20,{"spine":((0,0,0),(0,math.radians(30),0)),"upper_arm.R":((0,0,0),(math.radians(80),math.radians(-30),0)),"forearm.R":((0,0,0),(math.radians(65),0,0))})
    key_pose(arm,34,{})
    return action

def build_animations(arm):
    actions = [action_idle(arm), action_run(arm)]
    actions += [action_attack(arm, "Attack01", 1), action_attack(arm, "Attack02", -1), action_attack(arm, "Attack03", 1)]
    actions += [action_dodge(arm), action_skill(arm, "Skill01"), action_skill(arm, "Skill02")]
    return actions

# ---------- export ----------

def export():
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.context.view_layer.objects.active = bpy.data.objects.get("PlayerArmature")
    bpy.ops.export_scene.fbx(
        filepath=FBX_PATH,
        use_selection=False,
        object_types={"ARMATURE", "MESH"},
        add_leaf_bones=False,
        apply_scale_options="FBX_SCALE_ALL",
        bake_anim=True,
        bake_anim_use_all_actions=True,
        bake_anim_use_nla_strips=False,
        bake_anim_use_all_bones=True,
        axis_forward="-Z",
        axis_up="Y",
    )
    bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH)

def main():
    clear_scene()
    parts = build_character()
    arm = make_rig()
    bind_parts(parts, arm)
    build_animations(arm)

    # Put the armature at the origin and make it the active export root.
    bpy.context.view_layer.objects.active = arm
    arm.select_set(True)
    for obj in parts:
        obj.select_set(True)

    export()
    print("Player character generated:", FBX_PATH)

main()

import os
import re
from collections import defaultdict

def parse_unity_scene_detailed(file_path):
    """Parse Unity scene and extract complete GameObject/Component structure"""

    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Split by Unity object blocks
    blocks = re.split(r'--- !u!', content)

    gameobjects = {}
    transforms = {}
    components = {}

    for block in blocks[1:]:  # Skip first empty block
        lines = block.split('\n')
        if not lines:
            continue

        # Get object type and ID from first line (e.g., "1 &12345")
        header_match = re.match(r'(\d+)\s+&(\d+)', lines[0])
        if not header_match:
            continue

        obj_type = header_match.group(1)
        obj_id = header_match.group(2)

        # Parse GameObject (type 1)
        if obj_type == '1':
            name = ''
            components_list = []
            layer = ''
            active = True

            for line in lines:
                if line.strip().startswith('m_Name:'):
                    name = line.split(':', 1)[1].strip()
                elif line.strip().startswith('m_IsActive:'):
                    active = ('1' in line or 'true' in line.lower())
                elif line.strip().startswith('m_Layer:'):
                    layer = line.split(':', 1)[1].strip()
                elif '- component: {fileID:' in line:
                    comp_match = re.search(r'fileID:\s*(\d+)', line)
                    if comp_match:
                        components_list.append(comp_match.group(1))

            gameobjects[obj_id] = {
                'name': name,
                'component_refs': components_list,
                'layer': layer,
                'active': active
            }

        # Parse RectTransform (type 224) or Transform (type 4)
        elif obj_type in ['224', '4']:
            parent_id = '0'
            children = []

            for line in lines:
                if 'm_Father: {fileID:' in line:
                    parent_match = re.search(r'fileID:\s*(\d+)', line)
                    if parent_match:
                        parent_id = parent_match.group(1)
                elif '- {fileID:' in line:
                    child_match = re.search(r'fileID:\s*(\d+)', line)
                    if child_match:
                        children.append(child_match.group(1))

            transforms[obj_id] = {
                'type': 'RectTransform' if obj_type == '224' else 'Transform',
                'parent': parent_id,
                'children': children
            }

        # Parse other components
        else:
            comp_type = 'Unknown'
            class_id = ''

            # Map common type codes
            type_map = {
                '114': 'MonoBehaviour',
                '223': 'Canvas',
                '222': 'CanvasRenderer',
                '328': 'VideoPlayer',
                '20': 'Camera',
                '81': 'AudioListener'
            }

            comp_type = type_map.get(obj_type, f'Type_{obj_type}')

            # For MonoBehaviour, get the class identifier
            for line in lines:
                if line.strip().startswith('m_EditorClassIdentifier:'):
                    class_id = line.split(':', 1)[1].strip()
                    if '::' in class_id:
                        comp_type = class_id.split('::')[-1]
                    break

            components[obj_id] = {
                'type': comp_type,
                'type_code': obj_type,
                'class_id': class_id
            }

    # Build component mapping for GameObjects
    for obj_id, obj_data in gameobjects.items():
        obj_data['components'] = []
        for comp_ref in obj_data['component_refs']:
            if comp_ref in components:
                obj_data['components'].append(components[comp_ref]['type'])
            elif comp_ref in transforms:
                obj_data['components'].append(transforms[comp_ref]['type'])

    return gameobjects, transforms, components


def build_hierarchy_tree(gameobjects, transforms):
    """Build tree structure showing GameObject hierarchy"""

    # Find root objects (those with parent = 0)
    roots = []
    for trans_id, trans_data in transforms.items():
        if trans_data['parent'] == '0':
            # Find GameObject that owns this transform
            for obj_id, obj_data in gameobjects.items():
                if trans_id in obj_data['component_refs']:
                    roots.append((obj_id, trans_id))
                    break

    def build_tree(obj_id, trans_id, indent=0):
        lines = []
        if obj_id not in gameobjects:
            return lines

        obj = gameobjects[obj_id]
        prefix = "  " * indent

        # GameObject header
        name = obj['name'] if obj['name'] else "(unnamed)"
        active = "" if obj['active'] else " [INACTIVE]"
        lines.append(f"{prefix}• {name}{active}")

        # List components
        if obj['components']:
            for comp in obj['components']:
                lines.append(f"{prefix}  └─ {comp}")

        # Process children
        if trans_id in transforms:
            for child_trans_id in transforms[trans_id]['children']:
                # Find GameObject for this child transform
                for child_obj_id, child_obj_data in gameobjects.items():
                    if child_trans_id in child_obj_data['component_refs']:
                        lines.extend(build_tree(child_obj_id, child_trans_id, indent + 1))
                        break

        return lines

    all_lines = []
    for obj_id, trans_id in roots:
        all_lines.extend(build_tree(obj_id, trans_id))

    return all_lines


def main():
    scenes_dir = r"C:\Users\User\Robots Wearing Moustaches\Assets\Scenes"

    scene_files = [
        "LandingScreen.unity",
        "LobbyScreen.unity",
        "QuestionScreen.unity",
        "PictureQuestionScreen.unity",
        "EliminationScreen.unity",
        "VotingScreen.unity",
        "ResultsScreen.unity",
        "RoundArtScreen.unity",
        "HalftimeResultsScreen.unity",
        "BonusIntroScreen.unity",
        "BonusQuestionScreen.unity",
        "FinalResults.unity",
        "CreditsScreen.unity",
        "LoadingScreen.unity",
        "IntroVideoScreen.unity"
    ]

    output = []
    output.append("=" * 120)
    output.append("UNITY SCENE STRUCTURE - COMPLETE COMPONENT & HIERARCHY ANALYSIS")
    output.append("=" * 120)
    output.append("")

    for scene_file in scene_files:
        file_path = os.path.join(scenes_dir, scene_file)

        if not os.path.exists(file_path):
            output.append(f"\n⚠ WARNING: {scene_file} not found\n")
            continue

        output.append("\n" + "=" * 120)
        output.append(f"SCENE: {scene_file}")
        output.append("=" * 120)
        output.append("")

        gameobjects, transforms, components = parse_unity_scene_detailed(file_path)

        # Build hierarchy
        output.append("HIERARCHY:")
        output.append("-" * 120)
        hierarchy = build_hierarchy_tree(gameobjects, transforms)
        output.extend(hierarchy)
        output.append("")

        # Statistics
        output.append("STATISTICS:")
        output.append("-" * 120)
        output.append(f"Total GameObjects: {len(gameobjects)}")
        output.append(f"Total Transforms/RectTransforms: {len(transforms)}")
        output.append(f"Total Components: {len(components)}")
        output.append("")

        # Component summary
        comp_types = defaultdict(int)
        for comp in components.values():
            comp_types[comp['type']] += 1

        output.append("COMPONENT TYPES:")
        for comp_type, count in sorted(comp_types.items()):
            output.append(f"  • {comp_type}: {count}")
        output.append("")

        # Text elements (TextMeshProUGUI)
        text_count = comp_types.get('TMPro.TextMeshProUGUI', 0)
        output.append(f"TEXT ELEMENTS: {text_count} TextMeshProUGUI components")

        # Image elements
        img_count = comp_types.get('UnityEngine.UI.Image', 0)
        raw_img_count = comp_types.get('UnityEngine.UI.RawImage', 0)
        output.append(f"IMAGE ELEMENTS: {img_count} Image + {raw_img_count} RawImage = {img_count + raw_img_count} total")

        # Interactive elements
        button_count = comp_types.get('UnityEngine.UI.Button', 0)
        input_count = comp_types.get('TMPro.TMP_InputField', 0)
        output.append(f"INTERACTIVE ELEMENTS: {button_count} Buttons, {input_count} InputFields")

        output.append("")

    # Write output
    output_file = r"C:\Users\User\Robots Wearing Moustaches\unity_detailed_scene_report.txt"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write('\n'.join(output))

    print(f"Detailed analysis complete!")
    print(f"Output written to: {output_file}")
    print(f"Total lines in report: {len(output)}")


if __name__ == "__main__":
    main()

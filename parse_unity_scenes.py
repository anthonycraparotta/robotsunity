import os
import re
from collections import defaultdict

def parse_unity_scene(file_path):
    """Parse a Unity scene file and extract GameObject hierarchy with all components"""

    gameobjects = {}
    components = {}
    current_object = None
    current_component = None
    in_component = False

    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    i = 0
    while i < len(lines):
        line = lines[i].strip()

        # Detect GameObject definition
        if line.startswith('--- !u!1 &'):
            obj_id = line.split('&')[1]
            current_object = obj_id
            gameobjects[obj_id] = {
                'name': '',
                'components': [],
                'children': [],
                'layer': '',
                'active': True
            }
            in_component = False

        # Get GameObject name
        elif current_object and line.startswith('m_Name:'):
            name = line.split(':', 1)[1].strip()
            gameobjects[current_object]['name'] = name

        # Get GameObject active state
        elif current_object and line.startswith('m_IsActive:'):
            active = line.split(':', 1)[1].strip()
            gameobjects[current_object]['active'] = (active == '1')

        # Get GameObject layer
        elif current_object and line.startswith('m_Layer:'):
            layer = line.split(':', 1)[1].strip()
            gameobjects[current_object]['layer'] = layer

        # Get component references
        elif current_object and line.startswith('m_Component:'):
            # Read component list
            j = i + 1
            while j < len(lines) and lines[j].strip().startswith('- component:'):
                comp_line = lines[j].strip()
                match = re.search(r'fileID: (\d+)', comp_line)
                if match:
                    comp_id = match.group(1)
                    gameobjects[current_object]['components'].append(comp_id)
                j += 1

        # Get children references
        elif current_object and line.startswith('m_Children:'):
            j = i + 1
            while j < len(lines) and lines[j].strip().startswith('- {fileID:'):
                child_line = lines[j].strip()
                match = re.search(r'fileID: (\d+)', child_line)
                if match:
                    child_id = match.group(1)
                    gameobjects[current_object]['children'].append(child_id)
                j += 1

        # Detect component definitions (MonoBehaviour, RectTransform, etc.)
        elif line.startswith('--- !u!114 &') or line.startswith('--- !u!224 &') or \
             line.startswith('--- !u!223 &') or line.startswith('--- !u!222 &') or \
             line.startswith('--- !u!328 &'):
            parts = line.split('&')
            if len(parts) > 1:
                comp_id = parts[1].strip()
                comp_type_code = line.split('!u!')[1].split()[0]
                current_component = comp_id
                components[comp_id] = {
                    'type_code': comp_type_code,
                    'type_name': '',
                    'script_guid': '',
                    'class_identifier': ''
                }
                in_component = True

        # Get MonoBehaviour script reference
        elif in_component and current_component and line.startswith('m_Script:'):
            j = i + 1
            if j < len(lines):
                guid_line = lines[j].strip()
                if 'guid:' in guid_line:
                    match = re.search(r'guid: ([a-f0-9]+)', guid_line)
                    if match:
                        components[current_component]['script_guid'] = match.group(1)

        # Get component class identifier
        elif in_component and current_component and line.startswith('m_EditorClassIdentifier:'):
            identifier = line.split(':', 1)[1].strip()
            components[current_component]['class_identifier'] = identifier
            # Extract type name from class identifier
            if '::' in identifier:
                components[current_component]['type_name'] = identifier.split('::')[-1]

        # Map component type codes to names
        if in_component and current_component:
            type_code = components[current_component]['type_code']
            if type_code == '114':  # MonoBehaviour
                if not components[current_component]['type_name']:
                    components[current_component]['type_name'] = 'MonoBehaviour'
            elif type_code == '224':
                components[current_component]['type_name'] = 'RectTransform'
            elif type_code == '223':
                components[current_component]['type_name'] = 'Canvas'
            elif type_code == '222':
                components[current_component]['type_name'] = 'CanvasRenderer'
            elif type_code == '328':
                components[current_component]['type_name'] = 'VideoPlayer'

        i += 1

    return gameobjects, components


def build_hierarchy(gameobjects, components, root_ids):
    """Build hierarchical structure starting from root GameObjects"""

    def print_gameobject(obj_id, indent=0):
        if obj_id not in gameobjects:
            return []

        obj = gameobjects[obj_id]
        lines = []
        prefix = "  " * indent

        # Print GameObject info
        active_str = "" if obj['active'] else " (INACTIVE)"
        lines.append(f"{prefix}GameObject: {obj['name']}{active_str}")

        # Print components
        if obj['components']:
            lines.append(f"{prefix}  Components:")
            for comp_id in obj['components']:
                if comp_id in components:
                    comp = components[comp_id]
                    comp_name = comp['type_name'] if comp['type_name'] else f"Component_{comp['type_code']}"
                    lines.append(f"{prefix}    - {comp_name}")

        # Print children recursively
        if obj['children']:
            for child_id in obj['children']:
                lines.extend(print_gameobject(child_id, indent + 1))

        return lines

    all_lines = []
    for root_id in root_ids:
        all_lines.extend(print_gameobject(root_id))

    return all_lines


def find_root_objects(gameobjects):
    """Find root GameObjects (those that are not children of any other GameObject)"""
    all_children = set()
    for obj_id, obj_data in gameobjects.items():
        all_children.update(obj_data['children'])

    roots = [obj_id for obj_id in gameobjects.keys() if obj_id not in all_children]
    return roots


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

    output_lines = []
    output_lines.append("=" * 100)
    output_lines.append("UNITY SCENE COMPONENT ANALYSIS - COMPLETE REPORT")
    output_lines.append("=" * 100)
    output_lines.append("")

    for scene_file in scene_files:
        file_path = os.path.join(scenes_dir, scene_file)

        if not os.path.exists(file_path):
            output_lines.append(f"\n[WARNING] {scene_file} not found\n")
            continue

        output_lines.append("\n" + "=" * 100)
        output_lines.append(f"SCENE: {scene_file}")
        output_lines.append("=" * 100)
        output_lines.append("")

        gameobjects, components = parse_unity_scene(file_path)
        roots = find_root_objects(gameobjects)

        # Build and print hierarchy
        hierarchy_lines = build_hierarchy(gameobjects, components, roots)
        output_lines.extend(hierarchy_lines)
        output_lines.append("")

        # Print statistics
        output_lines.append(f"Statistics:")
        output_lines.append(f"  Total GameObjects: {len(gameobjects)}")
        output_lines.append(f"  Total Components: {len(components)}")
        output_lines.append("")

        # Component type summary
        comp_types = defaultdict(int)
        for comp in components.values():
            comp_type = comp['type_name'] if comp['type_name'] else f"Unknown_{comp['type_code']}"
            comp_types[comp_type] += 1

        output_lines.append("Component Types:")
        for comp_type, count in sorted(comp_types.items()):
            output_lines.append(f"  {comp_type}: {count}")
        output_lines.append("")

    # Write output to file
    output_file = r"C:\Users\User\Robots Wearing Moustaches\unity_scene_analysis.txt"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write('\n'.join(output_lines))

    print(f"Analysis complete! Output written to: {output_file}")
    print(f"Total lines: {len(output_lines)}")


if __name__ == "__main__":
    main()

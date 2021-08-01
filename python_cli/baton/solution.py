# Some credit: https://github.com/jhandley/pyvcproj
"""Visual Studio Solution File."""

import io, os, re, codecs
from dataclasses import dataclass

class SolutionFileError(Exception):
    pass

@dataclass
class GlobalSection:
    def __init__(self, match, lines : 'list[str]'):
        self.name : str = match[1]
        self.when : str = match[2]
        self.lines = lines

    HEADER_REGEX = re.compile(r"""\s*GlobalSection\(([^\)]+)\) = (.+)""")
    END_REGEX    = re.compile(r"""\s*EndGlobalSection""")

    @staticmethod
    def parse(line : str, file : io.BufferedReader) -> 'GlobalSection':
        header_match = GlobalSection.HEADER_REGEX.match(line)
        if not header_match: return None

        lines = []
        result = GlobalSection(header_match, lines)

        while True:
            line = file.readline().decode('utf-8')
            if line is None:
                raise SolutionFileError(f"Unexpected end of file while reading global section {result.name}")
            
            end_match = GlobalSection.END_REGEX.match(line)
            if end_match:
                break

            lines.append(line)

        return result

    def write_to(self, file : codecs.StreamWriter):
        GlobalSection.static_write_to(file, self.name, self.when, self.lines)
        # file.write(f"\tGlobalSection({self.name}) = {self.when}\n")
        # for line in self.lines: file.write(line)
        # file.write("\tEndGlobalSection\n")

    @staticmethod
    def static_write_to(file : codecs.StreamWriter, name, when, lines):
        file.write(f"\tGlobalSection({name}) = {when}\n")
        for line in lines: file.write(line)
        file.write("\tEndGlobalSection\n")


@dataclass
class Project:
    def __init__(self, match, dependencies):
        self.solution_guid : str = match[1]
        self.name          : str = match[2]
        self.relative_path : str = match[3]
        self.guid          : str = match[4]
        self.dependencies  = dependencies

    
    HEADER_REGEX        = re.compile(r'Project\("\{([^\}]+)\}"\)[\s=]+"([^\"]+)",\s"([^\"]+)", "(\{[^\}]+\})"')
    END_REGEX           = re.compile(r"""\s*EndProject""")
    END_PROJECT_SECTION = re.compile(r"""\s*EndProjectSection""")
    DEPENDENCY_REGEX    = re.compile(r"""\s*(\{[A-Za-z0-9-]+\})\s*=\s*(\{[A-Za-z0-9-]+\})""")
    DEPENDENCIES_SECTION_REGEX = re.compile(r"""\s*ProjectSection\(ProjectDependencies\) = postProject""")

    @staticmethod
    def parse(line : str, file : io.BufferedReader) -> 'Project':
        header_match = Project.HEADER_REGEX.match(line)
        if not header_match: return None
        
        dependencies = []
        result = Project(header_match, dependencies)
        
        line = file.readline().decode('utf-8')
        dependencies_section_match = Project.DEPENDENCIES_SECTION_REGEX.match(line)
        if dependencies_section_match:
            while True:
                line = file.readline().decode('utf-8')
                if line is None:
                    raise SolutionFileError(f"Unexpected end of file while reading project {result.name}")

                dependency_match = Project.DEPENDENCY_REGEX.match(line)
                if dependency_match:
                    # Both first and second matches contain the id of the project
                    dependencies.append(dependency_match[1])
                    continue

                dependencies_end_match = Project.END_PROJECT_SECTION.match(line)
                if dependencies_end_match:
                    break

                raise SolutionFileError(f"Expected either end of section or another dependency while reading project {result.name}")
                
        project_end_match = Project.END_REGEX.match(line)
        if not project_end_match:
            raise SolutionFileError(f"Expected end of project file while reading project {result.name}")

        return result

    def write_to(self, file : codecs.StreamWriter, path_to_previous_solution : str, solution_guid : str):
        new_path = os.path.join(path_to_previous_solution, self.relative_path) if path_to_previous_solution else self.relative_path
        solution_guid = solution_guid or self.solution_guid
        
        file.write(f'Project("{solution_guid}") = "{self.name}", "{new_path}", "{self.guid}"\n')

        if len(self.dependencies) > 0:
            file.write("\tProjectSection(ProjectDependencies) = postProject\n")
            
            for guid in self.dependencies:
                file.write(f'\t\t{guid} = {guid}\n')

            file.write("\tEndProjectSection\n")

        file.write("EndProject\n")

        
class Solution(object):
    """Visual C++ solution file (.sln)."""

    def __init__(self, projects : 'list[Project]', global_sections : 'dict[str, GlobalSection]', filename : str = None):
        """Create a Solution instance for solution file *name*."""
        self.projects        = projects or []
        self.global_sections = global_sections or {}
        self.filename        = filename

    @staticmethod
    def read(filename : str) -> 'Solution':
        projects = []
        global_sections = {}

        with open(filename, 'rb') as f:
            line = f.readline()

            # Expect the first line to be empty
            # if line != '\n' and line != '\:
            #     raise SolutionFileError(f'The first line must be empty, got {line}')
            
            # Read the Projects section
            while True:
                line = f.readline().decode('utf-8')

                # We're reading the project sections, so the file cannot end just yet
                if line is None:
                    raise SolutionFileError('Unexpected end of file while reading prjects')

                # Global section start
                if line.startswith("Global"):
                    break

                project = Project.parse(line, f)
                if project is not None:
                    projects.append(project)
                
            # Read the Globals sections
            while True:                          
                line = f.readline().decode('utf-8')

                if line is None:
                    raise SolutionFileError("Missing end global")

                if line.startswith("EndGlobal"):
                    break

                section = GlobalSection.parse(line, f)
                if section is not None:
                    global_sections[section.name] = section
                    continue
                
                # No lines outside globals are allowed
                raise SolutionFileError(f"Unexpected input {line}")
        
        return Solution(projects, global_sections, filename)
        
    def write_file(self, filename : str):
        """Save solution file."""
        with codecs.open(filename, "wb", "utf-8-sig") as file:
            self.write_to(file)
    
    @staticmethod
    def write_header_to(file):
        file.write("\n")
        file.write("Microsoft Visual Studio Solution File, Format Version 11.00\n")
        file.write("# Visual Studio 2010\n")

    def write_to(self, file):
        self.write_header_to(file)

        for project in self.projects:
            project.write_to(file)

        file.write("Global\n")
        
        for global_section in self.global_sections.values():
            global_section.write_to(file)

        file.write("EndGlobal\n")


def combine_solutions(inputs : 'list[str]', output : str) -> None:
    solutions = [Solution.read(file) for file in inputs]
    solution_guid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"

    with codecs.open(output, "wb", "utf-8-sig") as file:

        Solution.write_header_to(file)

        for solution in solutions:
            dirname = os.path.dirname(solution.filename)
            for project in solution.projects:
                project.write_to(file, dirname, solution_guid)
        
        file.write("Globals\n")

        PROJECT_CONF = 'ProjectConfigurationPlatforms'
        PROJECT_CONF_WHEN = 'postSolution'
        project_conf_lines = []
        for solution in solutions:
            if PROJECT_CONF in solution.global_sections:
                section = solution.global_sections.pop(PROJECT_CONF)
                project_conf_lines += section.lines

        SOLUTION_CONF = 'SolutionConfigurationPlatforms'
        SOLUTION_CONF_WHEN = 'preSolution'
        unique_solution_configurations = set()
        for solution in solutions:
            if SOLUTION_CONF in solution.global_sections:
                section = solution.global_sections.pop(SOLUTION_CONF)
                unique_solution_configurations.update(section.lines)

        GlobalSection.static_write_to(file, SOLUTION_CONF, SOLUTION_CONF_WHEN, unique_solution_configurations)
        GlobalSection.static_write_to(file, 'SolutionProperties', 'preSolution', ['\t\tHideSolutionNode = FALSE\n'])
        GlobalSection.static_write_to(file, PROJECT_CONF, PROJECT_CONF_WHEN, project_conf_lines)
        
        merged_others : dict[str, GlobalSection] = {}
        for solution in solutions:
            for key in solution.global_sections.keys():
                if key in merged_others:
                    merged_others[key].lines += solution.global_sections[key].lines
                else:
                    merged_others[key] = solution.global_sections[key]
        for global_section in merged_others.values():
            global_section.write_to(file)

        file.write("EndGlobals\n")
        
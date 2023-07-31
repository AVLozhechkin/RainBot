from pathlib import Path
from zipfile import ZipFile
import os
import shutil
import xml.etree.ElementTree as ET

DEPENDENCY = '..\RainBot.Core\RainBot.Core.csproj'
CORE_PROJECT_NAME = 'RainBot.Core'
TESTS_PROJECT_NAME = 'RainBot.Tests'

def prepare(zips: Path):    
    if os.path.exists(zips):
        shutil.rmtree(zips)
    os.mkdir(zips)


def copy(src_folder: Path, zip_folder: Path):    
    for folder in os.listdir(src_folder):
        if folder.startswith('.') or folder == TESTS_PROJECT_NAME:
            continue
        path_to_folder = src_folder.joinpath(folder)
        
        if os.path.isfile(path_to_folder):
            continue
        
        path_to_copy = zip_folder.joinpath(folder)
        
        files_in_folder = os.walk(path_to_folder)
        
        os.mkdir(path_to_copy)
        
        for address, _, files in files_in_folder:
            if '\\bin' not in address and '\\obj' not in address:
                for file in files:
                    if not Path.exists(path_to_copy):
                        os.mkdir(path_to_copy)
                    shutil.copyfile(address + '/' + file, path_to_copy.joinpath(file))

def removeDependency(zip_folder: Path):
    files = list(zip_folder.rglob('*.csproj'))
    
    for file in files:
        root_node = ET.parse(file).getroot()
        
        for item_group in root_node.iter('ItemGroup'):
            for project_reference in item_group:
                if project_reference.attrib['Include'] == DEPENDENCY:
                    item_group.remove(project_reference)
                    break
            if len(item_group) == 0:
                root_node.remove(item_group)
        with open(file, 'w') as f:
            f.write(ET.tostring(root_node, encoding='unicode'))

def addCoreFiles(zips_folder: Path):
    projectFolders = os.listdir(zips_folder)
    projectFolders.remove(CORE_PROJECT_NAME)
    
    core_project_folder = zips_folder.joinpath(CORE_PROJECT_NAME)
    core_files = os.listdir(core_project_folder)
    core_files.remove(CORE_PROJECT_NAME + '.csproj')
    
    for project in projectFolders:
        project_folder = zips_folder.joinpath(project)
        for core_file in core_files:
            shutil.copy(core_project_folder.joinpath(core_file), project_folder)

def zipProjects(zip_folder: Path): 
    for folder in os.listdir(zip_folder):
        if folder == CORE_PROJECT_NAME:
            continue
        zip = ZipFile(zip_folder.joinpath(folder + '.zip'), "w")
        pathToFolder = zip_folder.joinpath(folder)
        for file in os.listdir(pathToFolder):
            if file.endswith('.cs') or file.endswith('.csproj'):
                zip.write(pathToFolder.joinpath(file), file)
        zip.close()

def removeProjectCopies(zip_folder:Path):
    for obj in os.listdir(zip_folder):
        if obj.endswith('.zip'):
            continue
        shutil.rmtree(zip_folder.joinpath(obj))
    
    
deployment_folder = Path(os.getcwd())
src_folder = deployment_folder.parents[1].joinpath('src')
zips_folder = deployment_folder.parents[0].joinpath("zips")

prepare(zips_folder)
copy(src_folder, zips_folder)
removeDependency(zips_folder)
addCoreFiles(zips_folder)
zipProjects(zips_folder)
removeProjectCopies(zips_folder)
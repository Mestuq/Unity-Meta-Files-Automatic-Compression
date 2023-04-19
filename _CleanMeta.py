#This script removes all meta files in project
import glob, sys, os, pathlib, shutil
os.chdir(os.path.dirname(sys.argv[0]))
print('------Deleting Now------')
for path, subdirs, files in os.walk(pathlib.Path()):
    for name in files:
        if name.endswith('.meta'):
            file=os.path.join(path, name)
            os.remove(file)
print('SUCCESS')
input("Press Enter to continue...")
#This script compresses all ".meta" files in one file "MetaFileCompressed.zip"
import glob, sys, os, pathlib, shutil
os.chdir(os.path.dirname(sys.argv[0]))
if os.path.exists('.\\ceche'):
    print('Deleting old ceche')
    shutil.rmtree('.\\ceche')
if os.path.exists('.\\MeteFilesCompressed.zip'):
    print('Deleting old zip')
    os.remove('.\\MeteFilesCompressed.zip')
print('------Starting Now------')
for path, subdirs, files in os.walk(pathlib.Path()):
    for name in files:
        if name.endswith('.meta'):
            file=os.path.join(path, name)
            if file[0:7] != ".\\ceche":
                copyName='ceche' + file[1:]
                os.makedirs(os.path.dirname(copyName), exist_ok=True)
                shutil.copy(file, copyName)
                print(file)
print('------Compressing-------')
shutil.make_archive('.\\MeteFilesCompressed', 'zip', '.\\ceche')
shutil.rmtree('.\\ceche')
print('SUCCESS')
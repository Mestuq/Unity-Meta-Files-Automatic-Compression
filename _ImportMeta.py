#This script extracting all ".meta" files in right destination
import glob, sys, os, pathlib, shutil, zipfile
os.chdir(os.path.dirname(sys.argv[0]))
print('------Importing Now------')
with zipfile.ZipFile('.//MeteFilesCompressed.zip', 'r') as zip_ref:
    zip_ref.extractall('.')
print('SUCCESS')
"""
 * Python script to Resize Image.
 *
 * usage: python ImageResize.py <filename> <size>
"""

#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 

import sys
import os
import os.path
import argparse
import re
import zipfile #引入zip管理模块

#定义一个函数，递归读取absDir文件夹中所有文件，并塞进zipFile文件中。参数absDir表示文件夹的绝对路径。
def writeAllFileToZip(absDir,zip):
    os.chdir(absDir)
    for f in os.listdir(absDir):
        absFile=os.path.join(absDir,f) #子文件的绝对路径
        if os.path.isdir(absFile): #判断是文件夹，继续深度读取。
            relFile=absFile[len(os.getcwd())+1:] #改成相对路径，否则解压zip是/User/xxx开头的文件。
            zip.write(relFile) #在zip文件中创建文件夹
            writeAllFileToZip(absFile,zip) #递归操作
        else: #判断是普通文件，直接写到zip文件中。
            relFile=absFile[len(os.getcwd())+1:] #改成相对路径
            zip.write(relFile)
    return

def writeFileToZip(filePath,zip):
    if os.path.isfile(filePath):
    #去除后缀
        (parent_path, file) = os.path.split(path)
        directory= parent_path+"/"
        suffix= os.path.splitext(file)[1]
        os.chdir(directory)
        print('directory:', directory)
        #判断是普通文件，直接写到zip文件中。
        relFile= filePath[len(os.getcwd())+1:] #改成相对路径
        print('relFile:', relFile)
        zip.write(relFile)
    return

parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
parser.add_argument('--out', type=str, default = None)

args = parser.parse_args()
path= args.path.replace("\\","/")
outPath = args.out.replace("\\","/")

print('path:', path)
print('outPath:', outPath)

if outPath==path:
    if os.path.isfile(path):
        #去除后缀
        (parent_path, file) = os.path.split(path)
        directory= parent_path
        suffix= os.path.splitext(file)[1]
        outPath= path.replace(suffix,".zip")
    else:
        (parent_path, file) = os.path.split(path)
        directory= parent_path
        suffix= os.path.splitext(file)[1]
        print('directory:', directory)
        print('file:', file)
        outPath= os.path.join(directory,file)+".zip"

# 得到进程当前工作目录
currentpath = os.getcwd()

outPath=outPath.replace("\\","/")

print('outPath:', outPath)

zip=zipfile.ZipFile(outPath,"w",zipfile.ZIP_DEFLATED)

if os.path.isfile(path):
    writeFileToZip(path,zip)
else:
    writeAllFileToZip(path,zip)

zip.close()
sys.stdin.flush()
os.chdir(currentpath)

print("Zip Done")
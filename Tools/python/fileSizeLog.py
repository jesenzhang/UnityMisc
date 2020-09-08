'''
Descripttion: https://github.com/jesenzhang/UnityMisc.git
version: 
Author: jesen.zhang
Date: 2020-09-08 09:27:05
LastEditors: jesen.zhang
LastEditTime: 2020-09-08 10:06:47
'''
#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 
#将文件中的文件按照大小排序

import os
import sys
import os.path
import argparse
import functools

parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
args = parser.parse_args()

def get_FileSize(filePath):
    fsize = os.path.getsize(filePath)
    fsize = fsize/float(1024 * 1024)
    return round(fsize, 2)

def cmp(a, b):
    if b[1] < a[1]:
        return -1
    if a[1] < b[1]:
        return 1
    return 0

def walkDirs(orignalPath,fileArray):
    for root, dirs, files in os.walk(orignalPath):
        for fileName in files:
            path= os.path.join(root,fileName)
            size = get_FileSize(path)
            tup = (path, size)
            fileArray.append(tup)
        for sub in dirs:
            subpath= os.path.join(root,sub)
            walkDirs(subpath,fileArray)
            
#原始图片资源的目录
orignalPath = args.path

# 得到进程当前工作目录
currentpath = os.getcwd()

fileArray = []

walkDirs(orignalPath,fileArray)
fileArray = sorted(fileArray, key=functools.cmp_to_key(cmp))

for item in fileArray:
    print(item[0] + " 文件大小：%.2f MB"%(item[1]))

sys.stdin.flush()
os.chdir(currentpath)
print("文件大小检查 Done")
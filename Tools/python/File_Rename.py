'''
@Descripttion: https://github.com/jesenzhang/UnityMisc.git
@version: 
@Author: jesen.zhang
@Date: 2020-07-16 08:40:39
LastEditors: jesen.zhang
LastEditTime: 2020-09-08 10:07:01
'''
#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 
#对文件下下的文件自动重命名

import re
import os
import os.path
import sys
import argparse

parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
parser.add_argument('--prefix', type=str, default = None)
parser.add_argument('--suffix', type=str, default = None)
parser.add_argument('--start', type=int, default = None)
args = parser.parse_args()

path=args.path.replace("\\","/")
prefix=args.prefix
suffix=args.suffix
start=args.start

if os.path.isfile(path):
    (parent_path, file) = os.path.split(path)
    fileList = [file]
else:
    fileList = os.listdir(path)
    parent_path = path 

# 得到进程当前工作目录
currentpath = os.getcwd()
# 将当前工作目录修改为待修改文件夹的位置

os.chdir(parent_path)

fileList.sort()

index = start
# 遍历文件夹中所有文件
for filePath in fileList:
    if os.path.isfile(filePath):
        oldname = filePath.lower()
        oldname = re.sub(' ','',oldname)
        #去除后缀
        imagename= os.path.splitext(oldname)[0]
        oldsuffix= os.path.splitext(oldname)[1]
        tsuffix =suffix
        #非法字符替换
        if suffix== "-1":
           tsuffix= oldsuffix
        fullname = os.path.join(path,prefix+ '%d'%index +tsuffix)
        # 文件重新命名
        os.rename(filePath, fullname)
        # 输出此文件夹中包含的文件名称
        print("修改前：" + filePath +" 修改后：" + fullname)
        index=index+1

sys.stdin.flush()

os.chdir(currentpath)

print("文件重命名 Done")
'''
@Descripttion: https://github.com/jesenzhang/UnityMisc.git
@version: 
@Author: jesen.zhang
@Date: 2020-07-31 10:06:49
@LastEditors: jesen.zhang
@LastEditTime: 2020-07-31 13:02:56
'''
#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 
#对图片进行压缩 使用pngquant

import re
import os
import sys
import os.path
import argparse
import platform

parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
parser.add_argument('--out', type=str, default = None)

args = parser.parse_args()

path = args.path.replace("\\","/")
out = args.out.replace("\\","/")

#判断目标是否目录 
if (os.path.exists(out)==False):
    os.makedirs(out)

fileList = os.listdir(path)

# 得到进程当前工作目录
currentpath = os.getcwd().replace("\\","/")

# 将当前工作目录修改为待修改文件夹的位置
os.chdir(path)

systemType = platform.system()

pngquant =  systemType=="Windows" and "../pngquant/pngquant.exe" or systemType=="Mac" and "../pngquant/pngquant" or "../pngquant/pngquant.exe"
pngquant = currentpath+"/"+pngquant
print("pngquant "+pngquant)
# 遍历文件夹中所有文件
for fileName in fileList:
     if os.path.isfile(fileName):
        print(fileName)
        # 匹配文件名正则表达式
        pat = "^[a-z0-9_]+\.(png)"
        # 进行匹配
        matchObj = re.match(pat, fileName)
        if matchObj!=None:
            print(os.system(pngquant +" --force  --skip-if-larger --verbose --speed=1 --quality=45-85 "+ fileName + " --output "+ os.path.join(out,fileName).replace("\\","/")))
       
sys.stdin.flush()

os.chdir(currentpath)

print("Done")
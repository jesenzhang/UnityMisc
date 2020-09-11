'''
@Descripttion: https://github.com/jesenzhang/UnityMisc.git
@version: 
@Author: jesen.zhang
@Date: 2020-07-31 10:06:49
LastEditors: jesen.zhang
LastEditTime: 2020-09-11 16:27:03
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

try:
    path = unicode(path, 'GB2312') # 经过编码处理
except:
    pass # python3 已经移除 unicode，而且默认是 utf8 编码，所以不用转

out=None
if args.out!=None:
    out = args.out.replace("\\","/")
    try:
        out = unicode(out, 'GB2312') # 经过编码处理
    except:
        pass # python3 已经移除 unicode，而且默认是 utf8 编码，所以不用转
    #判断目标是否目录 
    if not os.path.isfile(out):
        if (os.path.exists(out)==False):
            os.makedirs(out)


abs_file=__file__
print("abs path is %s" %(__file__))
abs_dir=abs_file[:abs_file.rfind("\\")]   # windows下用\\分隔路径，linux下用/分隔路径
print("abs path is %s" %(os.path.abspath(sys.argv[0])))

# 得到进程当前工作目录
currentpath = abs_dir;# os.getcwd().replace("\\","/")
systemType = platform.system()
pngquant =  systemType=="Windows" and "../pngquant/pngquant.exe" or systemType=="Mac" and "../pngquant/pngquant" or "../pngquant/pngquant.exe"
pngquant = currentpath+"/"+pngquant
print("pngquant "+pngquant)

if os.path.isfile(path):
    # parent_path = os.path.dirname(path)
    (parent_path, file) = os.path.split(path)
    # pattern = re.compile(r'([^<>/\\\|:""\*\?]+)\.\w+$')
    # filename = pattern.findall(path)
    fileList = [file]
     # 将当前工作目录修改为待修改文件夹的位置
    os.chdir(parent_path)
    print("parent_path "+parent_path)
else:
    fileList = os.listdir(path)
    # 将当前工作目录修改为待修改文件夹的位置
    os.chdir(path)
    print("parent_path "+path)


# 遍历文件夹中所有文件
for fileName in fileList:
     if os.path.isfile(fileName):
        # 匹配文件名正则表达式
        pat = "^[a-z0-9_]+\.(png)"
        # 进行匹配
        matchObj = re.match(pat, fileName)
        print(fileName) 
        if matchObj!=None:
            if out==None:
                print(os.system(pngquant +" --force  --skip-if-larger --verbose --speed=1 --quality=45-85  --ext=.png "+ fileName))
                print(os.system(pngquant +" --force  --skip-if-larger --verbose --ordered --speed=1 --quality=50-90  --ext=.png "+ fileName))
            else:
                outpath =os.path.join(out,fileName).replace("\\","/")
                print(os.system(pngquant +" --force  --skip-if-larger --verbose --speed=1 --quality=45-85 "+ fileName + " --output "+ outpath))
                print(os.system(pngquant +" --force  --skip-if-larger --verbose --ordered --speed=1 --quality=50-90 --ext=.png "+ outpath))
       
sys.stdin.flush()

os.chdir(currentpath)

print("Done")
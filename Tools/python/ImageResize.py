"""
 * Python script to Resize Image.
 *
 * usage: python ImageResize.py <filename> <size>
"""

#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 

from PIL import Image, ImageFilter
import sys
import os
import os.path
import argparse
import re

parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
parser.add_argument('--outPath', type=str, default = None)
parser.add_argument('--mode', type=int, default = 0)
parser.add_argument('--width', type=int, default = None)
parser.add_argument('--height', type=int, default = None)
args = parser.parse_args()

path=args.path.replace("\\","/")
width=args.width
height=args.height
outPath=args.outPath
mode=args.mode

def ResizeTexture(filePath,mode,w,h,outpath):
    OriImage = Image.open(filePath)
    #OriImage.show()
    basename = os.path.basename(filePath)
    imagename= os.path.splitext(basename)[0]
    suffix= os.path.splitext(basename)[1]

    if mode==0:
        newsize = (w, h) 
    elif mode==1:
        newsize = (int(OriImage.size[0]*(w*0.01)),int(OriImage.size[1]*(h*0.01)))

    # img=img.resize((size, size), Image.ANTIALIAS)重置图片大小。
    # 其中，第二个参数：
    # Image.NEAREST ：低质量
    # Image.BILINEAR：双线性
    # Image.BICUBIC ：三次样条插值
    # Image.ANTIALIAS：高质量

    blurImage = OriImage.resize(newsize,Image.BILINEAR)
    #blurImage.show()

    print(basename+" "+imagename+" "+suffix)

    #Save image
    #if outpath==filePath:
    #    outpath = outpath.replace(imagename,imagename+"_resize")
    blurImage.save(outpath)

    OriImage.close()
    blurImage.close()
    sys.stdin.flush()

if outPath==None:
    outPath = path

pat = "(.*)\.(png||jpg)$"

if os.path.isfile(path):
   
    (parent_path, fileName) = os.path.split(path)
    # 进行匹配
    matchObj = re.match(pat, fileName)
    if matchObj!=None:
        ResizeTexture(path,mode,width,height,outPath)
else:
    for root, dirs, files in os.walk(path):
        print('root_dir:', root)  # 当前目录路径
        print('sub_dirs:', dirs)  # 当前路径下所有子目录
        print('files:', files)  # 当前路径下所有非目录子文件

        for fileName in files:
            filePath = os.path.join(root,fileName).replace("\\","/")
            fout = filePath.replace(path,outPath).replace("\\","/")
            print("filePath "+filePath)
            print("outpath "+fout)
            # 进行匹配
            matchObj = re.match(pat, fileName)
            if matchObj!=None:
                ResizeTexture(filePath,mode,width,height,fout)

print("Resize Done")
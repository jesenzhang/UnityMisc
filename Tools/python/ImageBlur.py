"""
 * Python script to demonstrate Gaussian blur.
 *
 * usage: python ImageBlur.py <filename> <sigma>
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
parser.add_argument('--sigma', type=int, default = None)
parser.add_argument('--outPath', type=str, default = None)
args = parser.parse_args()

path=args.path.replace("\\","/")
sigma=args.sigma
outPath=args.outPath

# filename = sys.argv[1]
# sigma = float(sys.argv[2])
# outPath = sys.argv[3]

def BlurTexture(filePath ,sigma, outpath):
    OriImage = Image.open(filePath)
    #OriImage.show()
    basename = os.path.basename(filePath)
    imagename= os.path.splitext(basename)[0]
    suffix= os.path.splitext(basename)[1]

    #ImageFilter.BoxBlur(5)
    #ImageFilter.GaussianBlur(radius=2) 
    #ImageFilter.BLUR
    blurImage = OriImage.filter(ImageFilter.GaussianBlur(sigma))
    #blurImage.show()

    print(basename+" "+imagename+" "+suffix)

    #Save blur image
    if outpath==filePath:
        outpath = outpath.replace(imagename,imagename+"_blur")
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
        BlurTexture(path,sigma,outPath)
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
                BlurTexture(filePath,sigma,fout)

print("Blur Done")
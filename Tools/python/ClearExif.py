"""
 * Python script to Resize Image.
 *
 * usage: python ImageResize.py <filename> <size>
"""

#!/usr/bin/env python3 
# -*- coding: UTF-8 -*- 

import piexif,os,time
from PIL import Image, ImageFilter
import sys
import os
import os.path
import argparse
import re
from pyexiv2 import Image

parser = argparse.ArgumentParser(description='manual to this script')
parser.add_argument('--path', type=str, default = None)
args = parser.parse_args()

path=args.path.replace("\\","/")

# filename = sys.argv[1]
# sigma = float(sys.argv[2])
# outPath = sys.argv[3]

def ClearExif(filePath):
    img = Image(filePath)
    #piexif.remove(filePath)
    img.clear_exif()
    img.clear_iptc()
    img.clear_xmp()
    img.clear_comment()
    img.close()	

pat = "(.*)\.(png||jpg)$"

if os.path.isfile(path):
    (parent_path, fileName) = os.path.split(path)
    # 进行匹配
    matchObj = re.match(pat, fileName)
    if matchObj!=None:
        ClearExif(path)
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
                ClearExif(filePath)

print("ClearExif Done")
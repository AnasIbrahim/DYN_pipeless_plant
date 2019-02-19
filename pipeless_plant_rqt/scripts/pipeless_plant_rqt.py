#!/usr/bin/env python

import sys

from pipeless_plant_rqt.pipeless_plant_plugin import PipelessPlantPlugin
from rqt_gui.main import Main

plugin = 'pipeless_plant_rqt'
main = Main(filename=plugin)
sys.exit(main.main(standalone=plugin))


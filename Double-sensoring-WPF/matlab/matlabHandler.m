function [ meanPulse, meanBreath ] = matlabHandler( colorList1, colorList2, colorList3, zList )
%
h = figure('visible', 'off')

meanPulse = pulse(colorList1, colorList2, colorList3);

meanBreath = breath(zList);

saveas(h, 'pulseplot.png')
end


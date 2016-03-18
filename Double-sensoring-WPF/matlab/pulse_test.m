function [ meanPulse ] = pulse_test(colorList1, colorList2, colorList3)
% Beräknar och plottar medelpulsen över något tidsintervall.
% Tidsintervallet bestäms av indata till funktionen
fh=findall(0,'type','subplot(2,1,1)');
for i=1:length(fh)
     clo(fh(i));
end

colorList5 = colorList3;
colorList5(:,:,2) = colorList2;
colorList5(:,:,3) = colorList1;

%HB = (0.2126*colorList1+0.7152*colorList2+0.0722*colorList3);

GdivR = double(colorList2)./double(colorList1);
colorList5(:,:,4) = GdivR;

d = fdesign.bandpass('N,F3dB1,F3dB2', 10, 50/60, 70/60, 26);
hd = design(d,'butter');
fvtool(hd)
filtcolorList = filtfilt(hd.sosMatrix,hd.ScaleValues,colorList5(:,:,4));

samplesPerSecPulse = 26;

numberOfSamplesPulse = length(colorList1);
timeOfMeasurementPulse = numberOfSamplesPulse/samplesPerSecPulse;

[heightOfPeaksPulse, peakLocationPulse]=findpeaks(filtcolorList);
numberOfPeaksPulse=length(peakLocationPulse);

for j = 1:numberOfPeaksPulse
   if (heightOfPeaksPulse(j) <= 0)
      heightOfPeaksPulse(j) = 0;
      peakLocationPulse(j) = 0;
   end
end

% for j = 1:(numberOfPeaksPulse - 1)
%     if (peakLocationPulse(j) - peakLocationPulse(j + 1) >= -5)
%         heightOfPeaksPulse(j) = 0;
%       peakLocationPulse(j) = 0;
%    end
% end
heightOfPeaksPulse(heightOfPeaksPulse == 0) = [];
peakLocationPulse(peakLocationPulse == 0) = [];
numberOfPeaksPulse = length(peakLocationPulse);

bpmPulse = (numberOfPeaksPulse/timeOfMeasurementPulse)*60;
meanPulse = round(bpmPulse);
% colorList5(:,:,4) = HB;

puls_t=colorList5;
save puls_t.mat

    figure(1)
    subplot(2,1,1)
    %hold off
    plot(filtcolorList, 'red');
    hold on
    plot(peakLocationPulse, heightOfPeaksPulse, 'black o');
    grid on
    title({'BPM';meanPulse}, 'color', 'red', 'FontWeight', 'bold')
    hold all
    xlabel('tid [s/30]')
    ylabel('Pulskurva')
%end
%end
hold off
end



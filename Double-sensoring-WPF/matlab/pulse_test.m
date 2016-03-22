function [ meanPulse ] = pulse_test(colorList1, colorList2, colorList3)
% Beräknar och plottar medelpulsen över något tidsintervall.
% Tidsintervallet bestäms av indata till funktionen
fh=findall(0,'type','subplot(2,1,1)');
for i=1:length(fh)
     clo(fh(i));
end

colorList5 = colorList1;
colorList5(:,:,2) = colorList2;
colorList5(:,:,3) = colorList3;

% Här väljs vilken input man vill se på.
colorList = colorList5(:,:,1);

sampleRate = 26;

d = fdesign.bandpass('N,F3dB1,F3dB2', 10, 50/60, 70/60, sampleRate);
hd = design(d,'butter');
filtcolorList = filtfilt(hd.sosMatrix,hd.ScaleValues,colorList);

samplesPerSecPulse = 26;

numberOfSamplesPulse = length(colorList);
timeOfMeasurementPulse = numberOfSamplesPulse/samplesPerSecPulse;

[heightOfPeaksPulse, peakLocationPulse]=findpeaks(filtcolorList);
numberOfPeaksPulse=length(peakLocationPulse);

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



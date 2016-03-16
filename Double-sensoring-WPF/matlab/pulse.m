function [ meanPulse ] = pulse(colorList1, colorList2, colorList3)
% Beräknar och plottar medelpulsen över något tidsintervall.
% Tidsintervallet bestäms av indata till funktionen
fh=findall(0,'type','subplot(2,1,1)');
for i=1:length(fh)
     clo(fh(i));
end

colorList5 = colorList3;
colorList5(:,:,2) = colorList2;
colorList5(:,:,3) = colorList1;

X1 = colorList1-colorList2;
X2 = colorList1+colorList2-2*colorList3;
X1=X1 - mean(X1);
X2=X2 - mean(X2);
X2=std(X1)/std(X2) * X2;

HB=X1-X2;
HB=HB/std(HB);

colorList5(:,:,4) = HB;

puls1=colorList5;
save puls1.mat

for i = 1:4
    colorList = colorList5(:,:,i);
% Inställningar; välj dina inställnignar för koden här
samplesPerSecPulse = 30;
% Slut inställningar

d = fdesign.bandpass('N,F3dB1,F3dB2', 10, 30/60, 100/60, 30);
hd = design(d,'butter');
filtcolorList = filtfilt(hd.sosMatrix,hd.ScaleValues,colorList);



%% Info om mätdata
numberOfSamplesPulse = length(colorList);
timeOfMeasurementPulse = numberOfSamplesPulse/samplesPerSecPulse;
% Slut på info om mätdata

% % Gör längden på vectorns indata udda
% if (mod(numberOfSamplesPulse,2) == 0) % om colorList är jämn i längd
%     colorList = colorList(2:numberOfSamplesPulse);
%     numberOfSamplesPulse = length(colorList);
% end
% % Slut

%% Kontrollerar att listan är tillräckligt lång
if length(colorList)<4
    error('Not enough samples of pulse')
end
%Slut kontroll
% 
% %% Filtrera colorList med hjälp av högsta möjliga gradens
% % Savitzky Golay FIR-filter
% colorList=double(colorList); % Kanske ej behövs
% degreeOfPolynomialPulse = samplesPerSecPulse - 1;
% smoothColorList = sgolayfilt(colorList,degreeOfPolynomialPulse,numberOfSamplesPulse);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaksPulse, peakLocationPulse]=findpeaks(filtcolorList);
numberOfPeaksPulse=length(peakLocationPulse);
% Slut filtrering

%% Pulsens medelvärde över antal sekunder
bpmPulse = (numberOfPeaksPulse/timeOfMeasurementPulse)*60;
% Slut medelvärde över antal sekunder

% Utskrifter
meanPulse = round(bpmPulse);
% Slut utskrifter

if (i == 1)
    color = 'red';
elseif (i == 2)
    color = 'green';
elseif (i == 4)
    color = 'black';
else
    color = 'blue';
end

colorList55(:,:,i) = filtcolorList;
if (i == 4)
% Plot av filtrerad data
h = figure(1)
subplot(2,1,1)
%hold off
plot(filtcolorList, color);
hold on
plot(peakLocationPulse, heightOfPeaksPulse, 'black o');
grid on
title({'BPM';meanPulse}, 'color', 'red', 'FontWeight', 'bold')
hold all
xlabel('tid [s/30]')
ylabel('Pulskurva')
% Slut plot av filtrerad data
end
end
hold off
saveas(h, 'pulseplot.png')
end



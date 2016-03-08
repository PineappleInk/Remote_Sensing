function [x] = myfunc(a) 
dif = mean(a);
x = plot(a);
t = text(mean(a), 150, 'Subject is moving, waiting for stabilizing', 'Color', 'none', 'Fontsize', 16);

if (max(a)-min(a) >= 0.5)
{
	t.Color = 'red'
}
ylim([mean(a)-0.01 mean(a)+0.01])
end
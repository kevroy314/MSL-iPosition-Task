import numpy as np
import scipy.spatial.distance as distance

#For study
# items=[2, 2, 2, 2, 2, 4, 4, 4, 4, 4, 6, 6, 6, 6, 6, 8, 8, 8, 8, 8, 10, 10, 10, 10, 10]
# For practice
items = [2, 6, 10]
seconds_per_item = 3
delay = 5000
stimuli=150
item_size = (15, 15)
pos_range=(-90, 90)
item_counter = 1
distance_constraint = 15

def get_points(n, r, gen_timeout=10000):
    pts = []
    counter = 0
    while len(pts) < n:
        counter+=1
        if counter > gen_timeout:
            print('Unable to generate points. Distance constraint is too strict.')
            return None
        new_pt = np.random.rand(2)*(max(r)-min(r))+min(r)
        good_point = True
        for pt in pts:
            if distance.euclidean(pt, new_pt) < distance_constraint:
                good_point = False
                break
        if good_point:
            pts.append(new_pt)
            counter = 0
    pts = list(np.array(pts).flatten())
    return pts

for item in items:
    line = ''
    duration = item * seconds_per_item * 1000
    line += str(duration) + ' ' + str(delay) + ' ' item_size[0] + ' ' + item_size[1] + ' '
    line += ' '.join(['{:.0f}.png'.format(p) for p in range(item_counter, item+item_counter)]) + ' '
    item_counter += item
    line += ' '.join(['{:.0f}'.format(p) for p in get_points(item, pos_range)])
    print(line)
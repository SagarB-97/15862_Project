import exif
import glob
import pandas as pd
import os
import plotly.graph_objects as go

if __name__ == '__main__':
    df = []
    working_path = '/Users/skalasib/Documents/Courses/15862_ComputationalPhotography/Project/Source/data'

    directories = glob.glob(f'{working_path}/*')
    for directory in directories:
        images_paths = glob.glob(f'{directory}/*')
        for image_path in images_paths:
            with open(image_path, 'rb') as img_file:
                img = exif.Image(img_file)
            this_image_data = {
                'directory': image_path.split('/')[-2],
                'filename': image_path.split('/')[-1],
                'f_number': img.get('f_number'),
                'file_size': os.path.getsize(image_path)
            }
            df.append(this_image_data)

    df = pd.DataFrame(df)

    fig = go.Figure()
    for directory in df['directory'].unique():
        this_df = df[df['directory'] == directory].sort_values('f_number')
        fig.add_trace(go.Scatter(
            x = this_df['f_number'], 
            y = this_df['file_size'],
            name = directory,
            text = this_df['filename'],
            hovertemplate = 'File:%{text}<br>f_number:%{x}<br>y:%{y}'
        ))
        
    fig.show()
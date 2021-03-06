﻿using GlmNet;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.JOG;
using SharpGL.Shaders;
using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGLLinesSample
{
    public class GLController
    {
        #region fields
        LinesProgram _scProgram;
        mat4 _projectionMatrix = mat4.identity(),
            _modelviewMatrix = mat4.identity();
        mat3 _normalMatrix = mat3.identity();


        ModelviewProjectionBuilder _mvpBuilder = new ModelviewProjectionBuilder();
        #endregion fields

        #region properties
        public LinesProgram SCProgram
        {
            get { return _scProgram; }
            set { _scProgram = value; }
        }
        /// <summary>
        /// The matrix responsable for deforming the projection.
        /// </summary>
        public mat4 ProjectionMatrix
        {
            get { return _projectionMatrix; }
            set
            {
                _projectionMatrix = value;
                SCProgram.Projection = value;
            }
        }
        /// <summary>
        /// The projection matrix, responsable for transforming objects in the world.
        /// </summary>
        public mat4 ModelviewMatrix
        {
            get { return _modelviewMatrix; }
            set
            {
                _modelviewMatrix = value;
                SCProgram.Modelview = value;
            }
        }


        public OpenGL GL { get { return SceneControl.Gl; } }
        public OpenGLControlJOG SceneControl { get; set; }

        public ModelviewProjectionBuilder MvpBuilder
        {
            get { return _mvpBuilder; }
            set { _mvpBuilder = value; }
        }
        #endregion properties

        #region events
        #endregion events

        #region constructors
        #endregion constructors
        public void Init(object sender, OpenGLEventArgs args)
        {
            SceneControl = sender as OpenGLControlJOG;

            // Set up the view.
            MvpBuilder.FovRadians = (float)Math.PI / 2f; // Set FOV to 90°
            MvpBuilder.Far = 100f;
            MvpBuilder.Near = 0.01f;
            MvpBuilder.Width = (int)SceneControl.ActualWidth;
            MvpBuilder.Height = (int)SceneControl.ActualHeight;

            MvpBuilder.TranslationZ = -10;

            MvpBuilder.BuildPerspectiveProjection();
            MvpBuilder.BuildTurntableModelview();


            // Create a shader program.
            SCProgram = new LinesProgram(GL);
            ProjectionMatrix = MvpBuilder.ProjectionMatrix;
            ModelviewMatrix = MvpBuilder.ModelviewMatrix;


            AddData(GL);

            GL.Enable(OpenGL.GL_DEPTH_TEST);

            GL.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
        }
        public void Draw(object sender, OpenGLEventArgs args)
        {
            GL.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            // Add gradient background.
            SetVerticalGradientBackground(GL, new ColorF(255, 146, 134, 188), new ColorF(1f, 0, 1, 0));

            SCProgram.BindAll(GL);
        }
        public void Resized(object sender, OpenGLEventArgs args)
        {
            var control = sender as OpenGLControlJOG;

            MvpBuilder.Width = (int)control.ActualWidth;
            MvpBuilder.Height = (int)control.ActualHeight;

            MvpBuilder.BuildPerspectiveProjection();

            ProjectionMatrix = MvpBuilder.ProjectionMatrix;
        }

        private void AddData(OpenGL gl)
        {
            var axis = new Axis(2);
            var axisLineWidth = 4;

            LinesBufferGroup groupAxisX = new LinesBufferGroup(gl);
            LinesBufferGroup groupAxisY = new LinesBufferGroup(gl);
            LinesBufferGroup groupAxisZ = new LinesBufferGroup(gl);

            groupAxisX.LineWidth = axisLineWidth;
            groupAxisY.LineWidth = axisLineWidth;
            groupAxisZ.LineWidth = axisLineWidth;

            var vertsX = axis.LineX.Item1.to_array().Concat(axis.LineX.Item2.to_array()).ToArray();
            var vertsY = axis.LineY.Item1.to_array().Concat(axis.LineY.Item2.to_array()).ToArray();
            var vertsZ = axis.LineZ.Item1.to_array().Concat(axis.LineZ.Item2.to_array()).ToArray();

            groupAxisX.BufferData(gl, null, vertsX, new ColorF(255, 255, 0, 0));
            groupAxisY.BufferData(gl, null, vertsY, new ColorF(255, 0, 255, 0));
            groupAxisZ.BufferData(gl, null, vertsZ, new ColorF(255, 0, 0, 255));

            groupAxisX.PrepareVAO(gl, SCProgram);
            groupAxisY.PrepareVAO(gl, SCProgram);
            groupAxisZ.PrepareVAO(gl, SCProgram);

            SCProgram.AddBufferGroup(groupAxisX);
            SCProgram.AddBufferGroup(groupAxisY);
            SCProgram.AddBufferGroup(groupAxisZ);


            LinesBufferGroup groupGrid = new LinesBufferGroup(gl);
            var grid = new SquareGrid(5, 1);

            groupGrid.LineWidth = 1;

            //var vertsGrid = grid.Lines.SelectMany(x => x.Item1.to_array().Concat(x.Item2.to_array())).ToArray();
            var vertsGrid = grid.Lines.SelectMany(x => x.to_array()).ToArray();

            groupGrid.BufferData(gl, null, vertsGrid, new ColorF(255, 0, 0, 0));
            groupGrid.PrepareVAO(gl, SCProgram);

            SCProgram.AddBufferGroup(groupGrid);


        }

        public void RefreshModelview()
        {
            MvpBuilder.BuildTurntableModelview();
            ModelviewMatrix = MvpBuilder.ModelviewMatrix;
        }
        public void RefreshProjection()
        {
            MvpBuilder.BuildPerspectiveProjection();
            ProjectionMatrix = MvpBuilder.ProjectionMatrix;
        }

        /// <summary>
        /// Sets the background color, using a gradient existing from 2 colors
        /// </summary>
        /// <param name="gl"></param>
        private static void SetVerticalGradientBackground(OpenGL gl, ColorF colorTop, ColorF colorBot)
        {
            float topRed = colorTop.R;// / 255.0f;
            float topGreen = colorTop.G;// / 255.0f;
            float topBlue = colorTop.B;// / 255.0f;
            float botRed = colorBot.R;// / 255.0f;
            float botGreen = colorBot.G;// / 255.0f;
            float botBlue = colorBot.B;// / 255.0f;

            gl.Begin(OpenGL.GL_QUADS);

            //bottom color
            gl.Color(botRed, botGreen, botBlue);
            gl.Vertex(-1.0, -1.0);
            gl.Vertex(1.0, -1.0);

            //top color
            gl.Color(topRed, topGreen, topBlue);
            gl.Vertex(1.0, 1.0);
            gl.Vertex(-1.0, 1.0);

            gl.End();
        }
    }
}

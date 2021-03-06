﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Digging_Game_3
{
    public interface IMy3DObject
    {
        Model3D Model { get; }
    }
    public abstract class My3DObject : IMy3DObject
    {
        public Model3D Model { get; private set; }
        public Transform3D Transform
        {
            get { return Model.Transform; }
            set { Model.Transform = value; }
        }
        public Transform3D OriginTransform = new MatrixTransform3D();
        public List<Transform3D> SubTransforms { get; set; } = new List<Transform3D> { };
        public void UpdateTransform()
        {
            var transform = new MatrixTransform3D(OriginTransform.Value);
            foreach (var t in SubTransforms) transform.Matrix = t.Value * transform.Matrix;
            Transform = transform;
        }
        protected abstract Model3D CreateModel(params object[]vs);
        protected My3DObject(params object[]vs)
        {
            Model = CreateModel(vs);
        }
    }
}

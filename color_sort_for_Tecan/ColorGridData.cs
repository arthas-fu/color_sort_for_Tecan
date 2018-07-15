using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections;
using System.Threading;

namespace color_sort_for_Tecan
{
    struct SortTaskParam
    {
        public Task task;
        public BlockingCollection<Int32> blocking_collection;
        public LinkedList<Int32> result;
    }

    public class ColorGridData
    {
        private Int32 width;
        private Int32 height;
        private Int32 sort_x;
        private Int32 sort_y;
        public Byte[] random_data { get; set; }
        public Byte[] sorted_data { get; set; }
        private float[] hue_array { get; set; }
        private Random rd = new Random();
        private color_align align { get; set; }
        private SortTaskParam[] task_params;
        private Int32 task_count = 0;

        public enum color_align
        {
            none = 0,
            left = 1,
            top = 2,
            right = 3,
            buttom = 4,
        }

        public ColorGridData(Int32 width, Int32 height)
        {
            this.width = width;
            this.height = height;
            this.random_data = new byte[height * width * 4];
            this.sorted_data = new byte[height * width * 4];
            this.hue_array = new float[height * width];
        }

        public void generate_random_color_grid()
        {
            this.rd.NextBytes(this.random_data);
            this.generate_hue_array(this.random_data);

            this.sort_x = 0;
            this.sort_y = 0;
            this.align = color_align.none;
        }

        private void generate_hue_array(Byte[] color_array)
        {
            if (this.hue_array.LongLength * 4 != color_array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            for (Int32 i = 0; i < this.hue_array.Length; ++i)
            {
                this.hue_array[i] = this.calculate_hue(color_array[i * 4], color_array[i * 4 + 1], color_array[i * 4 + 2]);
            }
        }

        public float calculate_hue(Byte b, Byte g, Byte r)
        {
            Byte M = Math.Max(r, g);
            Byte m = Math.Min(r, g);
            Byte C = 0;
            float h = 0, hue = 0;

            M = Math.Max(M, b);
            m = Math.Min(m, b);

            C = (Byte)(M - m);

            if (0 == C)
            {
                hue = 0;
                return hue;
            }

            if (M == r)
            {
                h = ((float)(g - b) / (float)C) % 6;
            }
            else if (M == g)
            {
                h = ((float)(b - r) / (float)C) + 2;
            }
            else if (M == b)
            {
                h = ((float)(r - g) / (float)C) + 4;
            }

            hue = 60 * h;

            if (hue < 0)
            {
                hue += 360;
            }

            if (360 == hue)
            {
                hue = 0;
            }

            return hue;
        }

        public void color_sort_by_hue(color_align align, Int32 task_count)
        {
            Int32 hue_size = this.hue_array.Length, task_index = 0, color_index = 0;
            float devided_unit = (float)360 / task_count;
            LinkedListNode<Int32> node = null;
            Byte[] color_byte = new Byte[4];

            if (align == this.align)
            {
                return;
            }
            switch (align)
            {
                case color_align.top:
                case color_align.left:
                    this.sort_x = 0;
                    this.sort_y = 0;
                    break;
                case color_align.right:
                    this.sort_x = this.width - 1;
                    this.sort_y = 0;
                    break;
                case color_align.buttom:
                    this.sort_x = 0;
                    this.sort_y = this.height - 1;
                    break;
                default:
                    return;
            }

            this.task_params = new SortTaskParam[task_count];
            this.task_count = task_count;

            for (Int32 i = 0; i < task_count; ++i)
            {
                this.task_params[i].blocking_collection = new BlockingCollection<int>(100);
                this.task_params[i].task = Task.Factory.StartNew(obj => this.section_sorting(obj), new { i, task_count });
            }

            for (Int32 i = 0; i < task_count; ++i)
            {
                this.task_params[i].task.Wait();
                this.task_params[i].task.Dispose();
            }

            for (Int32 i = 0; i < task_count; ++i)
            {
                node = this.task_params[i].result.First;
                while (null != node)
                {
                    color_index = node.Value;
                    color_byte[0] = this.random_data[color_index * 4];
                    color_byte[1] = this.random_data[color_index * 4 + 1];
                    color_byte[2] = this.random_data[color_index * 4 + 2];
                    color_byte[3] = this.random_data[color_index * 4 + 3];

                    this.fill_sorted_data_pixels(align, color_byte);
                    node = node.Next;
                }
            }
            this.align = align;
        }

        public void color_sort_by_hue(color_align align)
        {
            if (align == this.align)
            {
                return;
            }
            switch (align)
            {
                case color_align.top:
                case color_align.left:
                    this.sort_x = 0;
                    this.sort_y = 0;
                    break;
                case color_align.right:
                    this.sort_x = this.width - 1;
                    this.sort_y = 0;
                    break;
                case color_align.buttom:
                    this.sort_x = 0;
                    this.sort_y = this.height - 1;
                    break;
                default:
                    return;
            }

            Int32 min_index = 0, max_length = this.hue_array.Length;
            Byte[] tmp_byte = new Byte[4];
            float tmp_float = 0;

            for (Int32 i = 0; i < max_length; ++i)
            {
                for (Int32 j = i + 1; j < max_length; ++j)
                {
                    if (this.hue_array[j] < this.hue_array[min_index])
                    {
                        min_index = j;
                    }
                }

                tmp_byte[0] = this.random_data[min_index * 4];
                tmp_byte[1] = this.random_data[min_index * 4 + 1];
                tmp_byte[2] = this.random_data[min_index * 4 + 2];
                tmp_byte[3] = this.random_data[min_index * 4 + 3];

                this.random_data[min_index * 4] = this.random_data[i * 4];
                this.random_data[min_index * 4 + 1] = this.random_data[i * 4 + 1];
                this.random_data[min_index * 4 + 2] = this.random_data[i * 4 + 2];
                this.random_data[min_index * 4 + 3] = this.random_data[i * 4 + 3];

                this.random_data[i * 4] = tmp_byte[0];
                this.random_data[i * 4 + 1] = tmp_byte[1];
                this.random_data[i * 4 + 2] = tmp_byte[2];
                this.random_data[i * 4 + 3] = tmp_byte[3];

                tmp_float = this.hue_array[min_index];
                this.hue_array[min_index] = this.hue_array[i];
                this.hue_array[i] = tmp_float;

                this.fill_sorted_data_pixels(align, tmp_byte);
            }

            this.align = align;
        }

        private void fill_sorted_data_pixels(color_align align, Byte[] color_byte)
        {
            if (color_byte.Length != 4 || this.sort_x < 0 || this.sort_x >= this.width || this.sort_y < 0 || this.sort_y >= this.height)
            {
                return;
            }

            Int32 pixel_index = this.sort_y * this.width + this.sort_x;

            this.sorted_data[pixel_index * 4] = color_byte[0];
            this.sorted_data[pixel_index * 4 + 1] = color_byte[1];
            this.sorted_data[pixel_index * 4 + 2] = color_byte[2];
            this.sorted_data[pixel_index * 4 + 3] = color_byte[3];

            switch (align)
            {
                case color_align.left:
                    this.sort_y++;
                    if (this.sort_y == this.height)
                    {
                        this.sort_x++;
                        this.sort_y = 0;
                    }
                    break;
                case color_align.top:
                    this.sort_x++;
                    if (this.sort_x == this.width)
                    {
                        this.sort_y++;
                        this.sort_x = 0;
                    }
                    break;
                case color_align.right:
                    this.sort_y++;
                    if (this.sort_y == this.height)
                    {
                        this.sort_x--;
                        this.sort_y = 0;
                    }
                    break;
                case color_align.buttom:
                    this.sort_x++;
                    if (this.sort_x == this.width)
                    {
                        this.sort_y--;
                        this.sort_x = 0;
                    }
                    break;
                default:
                    break;
            }
        }

        private void section_sorting(object obj)
        {
            var o = (dynamic)obj;
            Int32 task_index = o.i, task_count = o.task_count, data = 0, hue_size = this.hue_array.Length;
            BlockingCollection<Int32> bc = this.task_params[task_index].blocking_collection;
            LinkedList < Int32 > ret_list = this.task_params[task_index].result = new LinkedList<Int32>();
            LinkedListNode<Int32> node = null;
            float devided_unit = (float)360 / task_count, hue = 0, min_hue = task_index * devided_unit, max_hue = (task_index + 1)* devided_unit;

 
            for (Int32 i = 0; i < hue_size; ++i)
            {
                hue = this.hue_array[i];
                if (hue < min_hue || hue >= max_hue)
                {
                    continue;
                }
                data = i;

                node = ret_list.First;
                while (null != node)
                {
                    if (this.hue_array[node.Value] > this.hue_array[data])
                    {
                        ret_list.AddBefore(node, data);
                        break;
                    }
                    node = node.Next;
                }
                if (null == node)
                {
                    ret_list.AddLast(data);
                }
            }
        }
    }
}
